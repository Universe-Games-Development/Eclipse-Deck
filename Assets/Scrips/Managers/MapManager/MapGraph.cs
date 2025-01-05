using System.Collections.Generic;

public class MapGraph {
    public List<Node> Nodes = new List<Node>();

    public Node AddNode(RoomSO room) {
        Node newNode = new Node(room);
        Nodes.Add(newNode);
        return newNode;
    }

    public class Node {
        public RoomSO Room; // Комната
        public List<Node> ConnectedNodes = new List<Node>(); // Соседние комнаты

        public Node(RoomSO room) {
            Room = room;
        }

        public void AddConnection(Node targetNode) {
            ConnectedNodes.Add(targetNode);
        }
    }
}
