using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectDotsMinigame : MonoBehaviour, IHackMinigame
{
    [System.Serializable]
    public class DotNode
    {
        public int nodeID;
        public Button nodeButton;
        public Image nodeImage;
        public Color nodeColor;
        public bool isConnected;
    }

    [Header("Grid Nodes Setup")]
    public List<DotNode> dotNodes = new List<DotNode>();

    private DotNode selectedStartNode;
    private System.Action onSuccess;
    private System.Action onFailure;

    public void InitializeMinigame(System.Action onSolveSuccess, System.Action onSolveFailed)
    {
        onSuccess = onSolveSuccess;
        onFailure = onSolveFailed;

        foreach (var node in dotNodes)
        {
            node.isConnected = false;
            node.nodeImage.color = node.nodeColor;
            DotNode current = node;
            node.nodeButton.onClick.RemoveAllListeners();
            node.nodeButton.onClick.AddListener(() => OnNodeClicked(current));
        }
    }

    public void ResetMinigame()
    {
        selectedStartNode = null;
        foreach (var node in dotNodes)
        {
            node.isConnected = false;
        }
    }

    private void OnNodeClicked(DotNode clickedNode)
    {
        if (selectedStartNode == null)
        {
            selectedStartNode = clickedNode;
            Debug.Log($"Selected start node ID: {clickedNode.nodeID}");
            return;
        }

        if (selectedStartNode == clickedNode)
        {
            selectedStartNode = null; // Deselect
            return;
        }

        // Check if endpoints match color ID and are distinct nodes
        if (selectedStartNode.nodeID == clickedNode.nodeID)
        {
            selectedStartNode.isConnected = true;
            clickedNode.isConnected = true;
            Debug.Log($"Nodes connected! ID: {clickedNode.nodeID}");

            selectedStartNode = null;
            CheckCompletion();
        }
        else
        {
            Debug.Log("Invalid connection!");
            selectedStartNode = null;
            onFailure?.Invoke();
        }
    }

    private void CheckCompletion()
    {
        foreach (var node in dotNodes)
        {
            if (!node.isConnected) return;
        }

        Debug.Log("Connect the Dots solved!");
        onSuccess?.Invoke();
    }
}
