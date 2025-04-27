using DotRecast.Core.Numerics;

namespace Scripts.Structure
{
    public static class StateNavHelper
    {
        /// <summary>
        /// Finds a good spot for any structure type.
        /// </summary>
        /// <typeparam name="T">Structure type (e.g., FoodArea, Exit, Stage).</typeparam>
        /// <param name="agent">The agent that is moving.</param>
        /// <param name="currentStructure">The current structure being used (ref).</param>
        /// <returns>True if a good spot was found, false otherwise.</returns>
        public static bool FindSpot<T>(AgentStatsController agent, ref Structure<T> currentStructure) where T : Structure<T>
        {
            if (agent.ID == null)
                return false;

            // Make sure agent has a structure
            if (currentStructure == null)
            {
                currentStructure = Structure<T>.FindNearest(agent.ID.npos);
                if (currentStructure == null)
                    return false;

                agent.Crowd.SetTarget(agent.ID, agent.NextRef.Ref, agent.NextRef.Pos);
                return false;
            }

            // If inside structure area
            if (currentStructure.EnteredArea(agent.ID.npos))
            {
                // If currently targeting the center (structure.Ref) or wrong target, pick best spot
                if ( agent.NextRef.Ref != agent.ID.targetRef )
                {
                    agent.NextRef = Structure<T>.GetBestSpot(agent.ID.npos, currentStructure, out Structure<T> maybeNewStructure);

                    if (maybeNewStructure != currentStructure)
                        currentStructure = maybeNewStructure;

                    agent.Crowd.SetTarget(agent.ID, agent.NextRef.Ref, agent.NextRef.Pos);
                }
                // If close enough to the chosen good spot
                else if ( RcVec3f.Distance(agent.ID.npos, agent.NextRef.Pos) <= agent.AcceptedDist )
                    return true;
            }
            else
            {
                // If outside structure area, move toward structure center
                if ( agent.ID.targetRef != currentStructure.Ref )
                    agent.Crowd.SetTarget(agent.ID, currentStructure.Ref, currentStructure.Position);
            }

            return false;
        }
    }
}