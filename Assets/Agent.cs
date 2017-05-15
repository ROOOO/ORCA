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
        radius = GetComponent<MeshRenderer>().bounds.extents.magnitude;
        curVelocity = goalVec.normalized * maxVelocity;

        Debug.Log(maxVelocity);
        inited = true;
    }

    void getNeighbors()
    {
        ORCA.inst.getNeighbor(this, neighborsList);
    }

    Vector2 linearPrograming(List<Line> lines, Vector2 curV)
    {
        //curV = maxVelocity * prefVelocity;

        return curV;
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
            var relativeVel = neighbor.getCurVelocity() - curVelocity;
            var comRadius = neighbor.getRadius() + radius;

            Line line = new Line();
            Vector2 u = Vector2.zero;

            Vector2 vec = relativeVel - relativePos / Global.t;
            Vector2 normVec = vec.normalized;
            if (Vector2.Dot(vec, relativePos) < 0)
            {
                var relCircleDis = vec.magnitude - comRadius / Global.t;
                if (relCircleDis < 0)
                {
                    curVelocity += normVec * (-relCircleDis) / 2;
                    line.direction = vec;
                    line.point = curVelocity + 0.5f * -relCircleDis * vec;
                }
                else
                {
                    curVelocity = maxVelocity * prefVelocity;
                }
            }

            //Vector3 vec = relativeVel - relativePos / Global.t;
            //var mVec = Vector3.Magnitude(vec);
            //var dotProduct = Vector3.Dot(vec, relativePos);
            //if (dotProduct < 0 && Mathf.Pow(dotProduct, 2) > Mathf.Pow(comRadius, 2) * Mathf.Pow(mVec, 2))
            //{
            //    Vector3 normVec = vec.normalized;
            //    line.direction = new Vector3(normVec.y, -normVec.x);

            //    var relCircleDis = mVec - comRadius / Global.t;
            //    if (relCircleDis < 0)
            //    {
            //        //curVelocity += normVec * (-relCircleDis) / 2;
            //        u = normVec * -relCircleDis;
            //        curVelocity += 0.5f * u;
            //    }
            //    else
            //    {
            //        curVelocity = maxVelocity * prefVelocity;
            //    }
            //}
            //else
            //{
            //    float mEdge = Mathf.Sqrt(Mathf.Pow(comRadius, 2) + Mathf.Pow(mRelativePos, 2));
            //    float theta = Mathf.Asin(comRadius / mRelativePos);
            //    Vector3 edge = Vector3.zero;
            //    // right
            //    if (Global.det(vec, relativePos) < 0)
            //    {
            //        edge = -Global.rotate(relativePos, mEdge / mRelativePos, -comRadius / mRelativePos);
            //        Debug.Log(-(new Vector3(relativePos.x * mEdge + relativePos.y * comRadius, -relativePos.x * comRadius + relativePos.y * mEdge) / Mathf.Pow(mRelativePos, 2)).normalized);
            //        Debug.Log(edge.normalized);
            //        Debug.Log(Global.rotate(relativePos, -theta));
            //    }
            //    else
            //    {
            //        edge = Global.rotate(relativePos, mEdge / mRelativePos, comRadius / mRelativePos);
            //    }
            //    line.direction = edge.normalized;
            //    u = Vector3.Dot(relativeVel, line.direction) * line.direction - relativeVel;
            //    curVelocity += 0.5f * u;
            //}

            //line.point = curVelocity + 0.5f * u;

            //Debug.Log(curVelocity);

            lines.Add(line);
        }

        curVelocity = linearPrograming(lines, curVelocity);
    }
    void setNewPos()
    {
        var ds = curVelocity * Global.stepTime;
        Debug.Log(ds);
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
