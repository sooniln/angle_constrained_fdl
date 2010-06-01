using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soonil.GraphLib;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace Soonil.ForceDirectedLayout
{
    public class ForceDirectedLayout
    {
        private readonly FDLNode[] nodes;
        private readonly MutablePoint[] new_positions;
        private readonly double[] accels;
        private readonly FDLEdge[] constraints;

        private const double TIMESTEP = .1;
        private const double TIMESTEP_SQUARED = TIMESTEP * TIMESTEP;
        private const double DAMPING = .9;
        private const double K = 1.0;
        private const double K_SQUARED = K*K;

        private bool first_step = true;

        /// <summary>
        /// Initializes this force-directed layout. Assumes that graph has some
        /// reasonable initial node positions.
        /// </summary>
        /// <param name="graph">The graph to layout.</param>
        /// <param name="start_node">The node to start layout from.</param>
        public ForceDirectedLayout(IReadOnlyGraph<FDLNode, FDLEdge> graph, FDLNode start_node)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (start_node == null)
                throw new ArgumentNullException("start_node");
            if (!graph.ContainsNode(start_node))
                throw new ArgumentException("start_node must be in this graph");

            //initialize nodes array to only the reachable nodes
            ArrayList n = new ArrayList(graph.NodeCount);
            Algorithms.Algorithms.BreadthFirstSearch(graph, start_node, null, delegate(FDLNode node){
                n.Add(node);
            });
            nodes = new FDLNode[n.Count];
            n.CopyTo(nodes);

            new_positions = new MutablePoint[nodes.Length];
            accels = new double[nodes.Length];

            //summarize constraints
            HashSet<FDLEdge> h = new HashSet<FDLEdge>();
            for (int i = 0; i < nodes.Length; i++)
            {
                foreach (FDLEdge edge in nodes[i].OutEdges)
                {
                    DefaultEdge reverse = edge.Target.GetEdgeTo(edge.Source);
                    if(h.Contains(edge) || (reverse != null && h.Contains(reverse)))
                        continue;
                    h.Add(edge);
                }
            }
            constraints = new FDLEdge[h.Count];
            h.CopyTo(constraints);
        }

        /// <summary>
        /// Number of constraints found by the layout.
        /// </summary>
        public int NumConstraints
        {
            get { return constraints.Length; }
        }

        /// <summary>
        /// Performs one step in the layout process. Most uses will want to call this in a loop
        /// until there is little energy left in the system, or some amount of time has elapsed.
        /// </summary>
        /// <param name="handle_constraints">Whether to enforce constraints on this step of not.</param>
        /// <returns>An approximation of the total acceleration of the system...?</returns>
        public double Step()
        {
            // every thread must perform thread-safe operations only
            Parallel.For(0, nodes.Length, (i) =>
            {
                FDLNode node = nodes[i];
                MutableVector force = new MutableVector(Vector.ZERO_VECTOR);
                
                //sum forces
                for (int j = 0; j < nodes.Length; j++)
                {
                    if (j == i)
                        continue;

                    FDLNode other_node = nodes[j];
                    force.Add(NodeNodeForce(node, other_node));
                }

                //sum forces
                foreach (FDLEdge edge in node.InEdges)
                {
                    force.Add(EdgeForce(edge));
                    force.Add(EdgeTorque(edge));
                }

                //derive acceleration
                Vector acceleration = (Vector)force / node.Mass;
                accels[i] = acceleration.Magnitude;

                //derive position
                if (first_step)
                {
                    //eulers integration - assume old velocity was zero for first step
                    new_positions[i] = new MutablePoint(node.Position);
                    new_positions[i].Add(1.5 * acceleration * TIMESTEP_SQUARED);
                }
                else
                {
                    //verlet integration - requires constant timestep
                    new_positions[i].X = 0;
                    new_positions[i].Y = 0;

                    new_positions[i].Add((1 + DAMPING) * node.Position);
                    new_positions[i].Subtract(DAMPING * node.PreviousPosition);
                    new_positions[i].Add(acceleration * TIMESTEP_SQUARED);
                }
            }
            );

            // update node positions and sum accelerations
            double totalAccel = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Position = (Point)new_positions[i];
                totalAccel += accels[i];
            }

            first_step = false;

            // i'm not sure what to call this measure...
            // but it isn't time or mass biased, which seems to make it a good indicator of a 
            // local minima of... some measure of energy? My best guess is that it's a
            // really terrible and inaccurate approximation of the total acceleration of
            // the system. Approximation, since it sums forces only on a per node rather system
            // basis.
            return totalAccel;
        }

        /// <summary>
        /// Enforces all the angle constraints existing in layout section. The greater the
        /// repetition, the more inelastic the constraints.
        /// </summary>
        /// <param name="repetitions">How many repetitions of constraint enforcement to perform.</param>
        public void EnforceConstraints(int repetitions)
        {
            for (int i = 0; i < repetitions; i++)
            {
                for (int j = 0; j < constraints.Length; j++)
                {
                    FDLEdge edge = constraints[j];

                    double angle_delta = edge.AngleDelta;

                    // only enforce constraints on angles close to their ideal positions
                    // otherwise we run into some troublesome problems
                    if (angle_delta == 0.0 || Math.Abs(angle_delta) > Math.PI / 4.0)
                        continue;

                    double amount = Math.Abs((edge.Length * Math.Sin(angle_delta)) / 2.0);
                    double angle = edge.AngleIdeal - Math.Sign(angle_delta) * Math.PI / 2.0;

                    edge.Source.Position += Vector.FromPolar(amount, angle);
                    edge.Target.Position += Vector.FromPolar(-1 * amount, angle);

                    // sanity check - the constraint has made things better
                    Debug.Assert(Math.Abs(angle_delta) >= Math.Abs(edge.AngleDelta));
                }
            }
        }

        private static Vector NodeNodeForce(FDLNode node1, FDLNode node2)
        {
            // distance squared for a small perf improvement
            double distance_s = NodeDistanceSquared(node1, node2);

            // composite function, must be continuous
            if (distance_s >= 4.0 * K_SQUARED)
            {
                return Vector.ZERO_VECTOR;
            }
            else if (distance_s > K_SQUARED)
            {
                double distance = Math.Sqrt(distance_s);
                double repulsion = Math.Min(-1.0 * distance / K + 2.0, K);
                double angle = NodeAngle(node2, node1);
                return Vector.FromPolar(repulsion, angle);
            }
            else if (distance_s > 0)
            {
                double repulsion = Math.Min(K_SQUARED / distance_s, K);
                double angle = NodeAngle(node2, node1);
                return Vector.FromPolar(repulsion, angle);
            }
            else
            {
                return Vector.ZERO_VECTOR;
            }
        }

        private static Vector EdgeForce(FDLEdge edge)
        {
            double force = 5.0 * edge.LengthDelta / K;
            double angle = edge.Angle;
            return Vector.FromPolar(force, angle);
        }

        private static Vector EdgeTorque(FDLEdge edge)
        {
            double force = 10.0 * edge.AngleDelta / K;
            double angle = edge.Angle + Math.PI / 2.0;
            return Vector.FromPolar(force, angle);
        }

        private static double NodeDistanceSquared(FDLNode node1, FDLNode node2)
        {
            Point v1 = node1.Position;
            Point v2 = node2.Position;

            double dx = v1.X - v2.X;
            double dy = v1.Y - v2.Y;

            return dx * dx + dy * dy;
        }

        private static double NodeAngle(FDLNode from, FDLNode to)
        {
            FDLEdge e = from.GetEdgeTo(to) as FDLEdge;
            if (e != null)
                return e.Angle;

            return Vector.CalcAngle(from.Position, to.Position);
        }
    }
}
