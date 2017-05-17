using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VR;

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
        prefVelocity = maxVelocity * (goalPos - curPos).normalized;
        Vector2 newV = prefVelocity;

        // return curVelocity todo: 3D Linear Programming
        for (int i = 0; i < lines.Count; ++i)
        {
            var line1 = lines[i];
            var d = line1.direction;
            var p = line1.point;
            if (Global.det(d, p - curVelocity) > 0)
            {
                // ||d||^2 * t^2 + 2(dot(d, p)) * t + (||p||^2 - r^2) >= 0  available area in max speed
                // delta = b^2 - 4ac, ||d|| = 1
                var dp = Vector2.Dot(p, d);
                var delta = 4 * Mathf.Pow(dp, 2) - 4 * (p.sqrMagnitude - Mathf.Pow(maxVelocity, 2));
                if (delta < 0)
                {
                    newV = curVelocity;
                    break;
                }
                delta /= 4;

                // m1 = (-b - sqrt(delta)) / ||d||^2
                // m2 = (-b + sqrt(delta)) / ||d||^2
                // m1 <= m2
                var m1 = -dp - Mathf.Sqrt(delta);
                var m2 = -dp + Mathf.Sqrt(delta);

                // Cramer's rule
                // p = p1 + t1d1 = p2 + t2d2
                for (var j = 0; j < i; ++j)
                {
                    var line2 = lines[j];

                    // t1 = det(d2, p1 - p2) / det(d1, d2)
                    var t1y = Global.det(line1.direction, line2.direction);
                    var t1x = Global.det(line2.direction, p - line2.point);
                    // parallel
                    if (Mathf.Abs(t1y) < 0.001)
                    {
                        // on the other side of available area
                        if (t1x < 0)
                        {
                            newV = curVelocity;
                            return newV;
                        }
                        // todo
                        continue;
                    }

                    var t = t1y / t1x;
                    // right side of line1, mod m1
                    if (t1y < 0)
                    {
                        m1 = Mathf.Max(m1, t);
                    }
                    else
                    {
                        m2 = Mathf.Min(m2, t);
                    }

                    if (m1 > m2)
                    {
                        newV = curVelocity;
                        return newV;
                    }
                }

                var tPref = Vector2.Dot(d, prefVelocity - p);
                if (tPref > m2)
                {
                    newV = p + m2 * d;
                }
                else if (tPref < m1)
                {
                    newV = p + m1 * d;
                }
                else
                {
                    newV = p + tPref * d;
                }
                curVelocity = newV;
            }
        }

        return newV;
    }

    void computeNewVelocity()
    {
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

            if (comRadius < relativePos.magnitude)
            {
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
                    // project   u' = v * dot(u, v) / v^2
                    // v = ||line.direction|| = 1
                    u = Vector2.Dot(relativeVel, line.direction) * line.direction - relativeVel;
                }
            }
            else
            {
                Vector2 vec = relativeVel - relativePos / Global.stepTime;
                Vector2 normVec = vec.normalized;
                line.direction = new Vector2(normVec.y, -normVec.x);
                u = (comRadius / Global.stepTime - vec.magnitude) * normVec;
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
