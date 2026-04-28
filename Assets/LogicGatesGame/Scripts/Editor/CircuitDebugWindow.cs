using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using UnityEditor;
using UnityEngine;
using MsaglGraph = Microsoft.Msagl.Core.Layout.GeometryGraph;
using MsaglNode = Microsoft.Msagl.Core.Layout.Node;
using MsaglEdge = Microsoft.Msagl.Core.Layout.Edge;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace LogicGatesGame.Scripts.Editor
{
    public class CircuitDebugWindow : EditorWindow
    {
        private const float NodeRadius = 24f;
        private const float ColumnSpacing = 140f;
        private const float RowSpacing = 80f;
        private const float Margin = 40f;

        private CircuitController _controller;
        private Dictionary<int, Vector2> _layout = new();
        private HashSet<int> _lastIds = new();
        private bool _layoutDirty = true;
        private Vector2 _scroll;
        

        [MenuItem("LogicGatesGame/Circuit Debug View")]
        private static void Open() => GetWindow<CircuitDebugWindow>("Circuit");

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= Repaint;
            UnsubscribeController();
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            UnsubscribeController();
            _controller = null;
            _layout.Clear();
            _lastIds.Clear();
            _layoutDirty = true;
        }

        private void UnsubscribeController()
        {
            if (_controller != null)
                _controller.OnCircuitChanged -= OnCircuitChanged;
        }

        private void AcquireController()
        {
            if (_controller != null) return;
            _controller = FindFirstObjectByType<CircuitController>();
            if (_controller != null)
                _controller.OnCircuitChanged += OnCircuitChanged;
        }

        private void OnCircuitChanged()
        {
            _layoutDirty = true;
            Repaint();
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view the circuit.", MessageType.Info);
                return;
            }

            AcquireController();
            if (_controller == null)
            {
                EditorGUILayout.HelpBox("No CircuitController found in scene.", MessageType.Warning);
                return;
            }

            var nodes = _controller.Nodes;
            EnsureLayout(nodes);

            float maxX = _layout.Values.DefaultIfEmpty(Vector2.zero).Max(p => p.x) + Margin + NodeRadius;
            float maxY = _layout.Values.DefaultIfEmpty(Vector2.zero).Max(p => p.y) + Margin + NodeRadius;

            _scroll = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height),
                _scroll, new Rect(0, 0, maxX, maxY));

            DrawConnections(nodes);
            DrawNodes(nodes);

            GUI.EndScrollView();
        }

        private void EnsureLayout(IReadOnlyDictionary<int, Node> nodes)
        {
            var ids = new HashSet<int>(nodes.Keys);
            if (!_layoutDirty && ids.SetEquals(_lastIds)) return;

            _lastIds = ids;
            _layout = ComputeLayout(nodes);
            _layoutDirty = false;
        }

        private static Dictionary<int, Vector2> ComputeLayout(IReadOnlyDictionary<int, Node> nodes)
        {
            var result = new Dictionary<int, Vector2>();
            if (nodes.Count == 0) return result;

            var ms = new MsaglGraph();
            var map = new Dictionary<int, MsaglNode>();
            foreach (var n in nodes.Values)
            {
                var msNode = new MsaglNode(
                    CurveFactory.CreateRectangle(NodeRadius * 2, NodeRadius * 2, new MsaglPoint()),
                    n.Id);
                ms.Nodes.Add(msNode);
                map[n.Id] = msNode;
            }
            foreach (var n in nodes.Values)
                foreach (var o in n.Outputs)
                    ms.Edges.Add(new MsaglEdge(map[n.Id], map[o.Id]));

            var settings = new SugiyamaLayoutSettings
            {
                NodeSeparation = RowSpacing * 0.5,
                LayerSeparation = ColumnSpacing * 0.5,
            };
            LayoutHelpers.CalculateLayout(ms, settings, null);

            // MSAGL default Sugiyama: top-to-bottom. Swap axes + mirror X so we render right-to-left.
            double minOurX = double.MaxValue, maxOurX = double.MinValue, minOurY = double.MaxValue;
            foreach (var n in nodes.Values)
            {
                var c = map[n.Id].Center;
                if (c.Y < minOurX) minOurX = c.Y;
                if (c.Y > maxOurX) maxOurX = c.Y;
                if (c.X < minOurY) minOurY = c.X;
            }

            foreach (var n in nodes.Values)
            {
                var c = map[n.Id].Center;
                float fx = Margin + (float)(maxOurX - c.Y);
                float fy = Margin + (float)(c.X - minOurY);
                result[n.Id] = new Vector2(fx, fy);
            }
            return result;
        }

        private void DrawConnections(IReadOnlyDictionary<int, Node> nodes)
        {
            Handles.BeginGUI();
            foreach (var node in nodes.Values)
            {
                if (!_layout.TryGetValue(node.Id, out var from)) continue;
                foreach (var output in node.Outputs)
                {
                    if (!_layout.TryGetValue(output.Id, out var to)) continue;
                    Handles.DrawLine(new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0), 2f);
                }
            }
            Handles.EndGUI();
        }

        private void DrawNodes(IReadOnlyDictionary<int, Node> nodes)
        {
            Handles.BeginGUI();
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            foreach (var kv in nodes)
            {
                if (!_layout.TryGetValue(kv.Key, out var pos)) continue;
                var node = kv.Value;

                Color prev = Handles.color;
                Handles.color = ColorFor(node);
                Handles.DrawSolidDisc(new Vector3(pos.x, pos.y, 0), Vector3.forward, NodeRadius);
                Handles.color = Color.black;
                Handles.DrawWireDisc(new Vector3(pos.x, pos.y, 0), Vector3.forward, NodeRadius);
                Handles.color = prev;

                var rect = new Rect(pos.x - NodeRadius, pos.y - NodeRadius, NodeRadius * 2, NodeRadius * 2);
                GUI.Label(rect, $"#{node.Id}\n{TagFor(node)}", labelStyle);
            }
            Handles.EndGUI();
        }

        private static Color ColorFor(Node node)
        {
            return node switch
            {
                SourceNode => new Color(0.3f, 0.7f, 0.3f),
                SinkNode => new Color(0.8f, 0.3f, 0.3f),
                AndNode => new Color(0.3f, 0.5f, 0.8f),
                OrNode => new Color(0.4f, 0.6f, 0.9f),
                NotNode => new Color(0.7f, 0.4f, 0.8f),
                SimpleNode => new Color(0.5f, 0.5f, 0.5f),
                _ => Color.gray
            };
        }

        private static string TagFor(Node node)
        {
            return node switch
            {
                SourceNode => "SRC",
                SinkNode => "SNK",
                AndNode => "AND",
                OrNode => "OR",
                NotNode => "NOT",
                SimpleNode => "SIM",
                _ => "?"
            };
        }
    }
}
