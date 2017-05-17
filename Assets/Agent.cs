using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {
    bool inited = false;

    Vector2 initPos;
    Vector2 goalPos;
    Vector2 prefVelocity;
    float maxVelocity;
    List<Agent> neighborsList = new List<Agent>();
    List<Line> lines = new List<Line>();

    Vector2 curVelocity;
    Vector2 curPos;
    float radius;
    public Vector2 getCurVelocity()
    {
        return curVelocity;
    }
    public Vector2 getCurPos()
    {
        return curPos;
    }
    public float getRadius()
    {
        return radius;
    }

    public bool touchGoalPos()
    {
        var pos = transform.localPosition;
        return (new Vector2(pos.x, pos.y)  - goalPos).magnitude < 0.1f;
    }

	public void Init () {
        initPos = transform.localPosition;
        goalPos = -initPos;
	    var goalVec = goalPos - initPos;
        maxVelocity = goalVec.magnitude / (Global.prefTimes * Global.stepTime);
        curPos = initPos;
        //radius = GetComponent<MeshRenderer>().bounds.extents.magnitude / 2.0f;
	    radius = 0.7f;
        curVelocity = goalVec.normalized * maxVelocity;

        inited = true;
    }

    void getNeighbors()
    {
        ORCA.inst.getNeighbor(this, neighborsList);
    }

    Vector2 linearPrograming(List<Line> lines)
    {
        prefVelocity = maxVelocity * prefVelocity;
        Vector2 newV = prefVelocity;

        for (int i = 0; i < lines.Count; ++i)
        {
            var line = lines[i];
            var d = line.direction;
            var p = line.point;
            if (Global.det(d, p - curVelocity) > 0)
            {
                var pq = Vector2.Dot(p, d);
                if (p.sqrMagnitude - Mathf.Pow(pq, 2) > Mathf.Pow(maxVelocity, 2))
                {
                    newV = Vector2.zero; // todo: 3D Linear Programming
                    break;
                }

                newV = lines[i].point;
            }
        }

        return newV;
    }

    void computeNewVelocity()
    {
        prefVelocity = (goalPos - curPos).normalized;
        lines.Clear();

        for (var i = 0; i < neighborsList.Count; ++i)
        {
            var neighbor = neighborsList[i];

            var relativePos = neighbor.getCurPos() - curPos;
            var mRelativePos = relativePos.magnitude;
            var relativeVel = curVelocity - neighbor.getCurVelocity();
            var comRadius = neighbor.getRadius() + radius;

            Line line = new Line();
            Vector2 u = Vector2.zero;

            Vector2 vec = relativeVel - relativePos / Global.t;
            var mVec = vec.magnitude;
            var dotProduct = Vector2.Dot(vec, relativePos);
            if (dotProduct < 0 && Mathf.Pow(dotProduct, 2) > Mathf.Pow(comRadius, 2) * Mathf.Pow(mVec, 2))
            {
                Vector2 normVec = vec.normalized;
                line.direction = new Vector2(normVec.y, -normVec.x);
                u = normVec * (comRadius / Global.t - mVec);
            }
            else
            {
                float mEdge = Mathf.Sqrt(Mathf.Pow(mRelativePos, 2) - Mathf.Pow(comRadius, 2));
                Vector2 edge = Vector2.zero;
                if (Global.det(relativePos, vec) > 0)
                {
                    edge = Global.rotate(relativePos, mEdge / mRelativePos, comRadius / mRelativePos);
                }
                else
                {
                    edge = Global.rotate(relativePos, mEdge / mRelativePos, -comRadius / mRelativePos);
                    edge *= -1;
                }
                line.direction = edge.normalized;
                u = Vector2.Dot(relativeVel, line.direction) * line.direction - relativeVel;
            }

            line.point = curVelocity + 0.5f * u;
            lines.Add(line);
        }

        curVelocity = linearPrograming(lines);
    }
    void setNewPos()
    {
        var ds = curVelocity * Global.stepTime;
        transform.localPosition += new Vector3(ds.x, ds.y);
        curPos = transform.localPosition;
    }

    public void UpdateAgent () {
        if (!inited)
        {
            return;
        }

        getNeighbors();
        computeNewVelocity();
        setNewPos();
    }
}
