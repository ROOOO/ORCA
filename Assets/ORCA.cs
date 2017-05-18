using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ORCA : MonoBehaviour {

    public static ORCA inst;
    private int totalTimes = 0;

    List<Agent> agentList = new List<Agent>();

    public void getNeighbor(Agent agent, List<Agent> neighborsList)
    {
        neighborsList.Clear();
        Agent neighbor;
        for (var i = 0; i < agentList.Count; ++i)
        {
            neighbor = agentList[i];
            if (neighbor != agent && neighbor.getRadius() - agent.getRadius() <= Global.maxNeighborRange)
            {
                neighborsList.Add(agentList[i]);
            }
        }
    }

    bool finished = false;
    bool isFinished()
    {
        ++totalTimes;
        if (totalTimes > Global.maxTimes)
        {
            return true;
        }
        for (var i = 0; i < agentList.Count; ++i)
        {   
            if (!agentList[i].touchGoalPos())
            {
                return false;
            }
        }
        return true;
    }

    void updateAgents()
    {
        for (var i = 0; i < agentList.Count; ++i)
        {
            agentList[i].UpdateAgent();
        }
    }

	// Use this for initialization
	void Start () {
        int childCount = transform.childCount;
        for (var i = 0; i < childCount; ++i)
        {
            var comp = transform.GetChild(i).GetComponent<Agent>();
            if (comp != null && comp.gameObject.activeInHierarchy)
            {
                comp.Init();
                agentList.Add(comp);
            }
        }
        inst = this;
	    totalTimes = 0;
	}

	// Update is called once per frame
	void Update () {
        if (finished)
        {
            return;
        }
	    if (isFinished())
        {
            Debug.Log("Finished!!");
            finished = true;
            return;
        }
        updateAgents();
	}
}
