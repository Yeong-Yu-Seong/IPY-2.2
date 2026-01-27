using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportBoundsChecker : MonoBehaviour
{
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private LayerMask barrierLayers;
    [SerializeField] private BoxCollider playAreaBounds;
    [SerializeField] private float checkRadius = 0.3f;
    
    private Vector3 lastValidPosition;

    private void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = transform;
        }
        
        lastValidPosition = xrOrigin.position;
    }

    private void LateUpdate()
    {
        Vector3 currentPosition = xrOrigin.position;
        
        // Check if outside bounds
        if (playAreaBounds != null && !playAreaBounds.bounds.Contains(currentPosition))
        {
            ResetToLastValidPosition();
            return;
        }
        
        // Check if inside a barrier
        if (Physics.CheckSphere(currentPosition, checkRadius, barrierLayers, QueryTriggerInteraction.Ignore))
        {
            ResetToLastValidPosition();
            return;
        }
        
        lastValidPosition = currentPosition;
    }

    private void ResetToLastValidPosition()
    {
        CharacterController cc = xrOrigin.GetComponent<CharacterController>();
        
        if (cc != null)
        {
            cc.enabled = false;
            xrOrigin.position = lastValidPosition;
            cc.enabled = true;
        }
        else
        {
            xrOrigin.position = lastValidPosition;
        }
        
        Debug.Log("Teleport cancelled: Outside valid area");
    }

    private void OnDrawGizmosSelected()
    {
        if (playAreaBounds != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(playAreaBounds.bounds.center, playAreaBounds.bounds.size);
        }
    }
}