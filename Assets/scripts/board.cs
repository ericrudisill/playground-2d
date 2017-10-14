using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class board : MonoBehaviour
{

    public GameObject[] GelPrefabs;
    public int GelColumns = 7;
    public int GelRows = 8;
    public Vector2 GelScale = new Vector2(1.0f, 1.0f);
    public float Padding = 0.15f;
    public float TweenFallDuration = 0.20f;
    public float TweenRndColumn = 0.1f;
    public float TweenRndRow = 0.05f;
    public float DragDistance = 1.0f;
    public float SwipeAngleLimit = 30.0f;

    private float extentWidth = 0;
    private float extentHeight = 0;
    private float extentLeft = 0;
    private float extentTop = 0;
    private float gelZeroX = 0;
    private float gelZeroY = 0;
    private float offY = 0;
    private Vector2 swipeDir = Vector2.zero;
    

    private gel touchGel = null;
    private Vector2 touchStart = Vector2.zero;
    private bool touchTracking = false;

    // Use this for initialization
    void Start()
    {
        drawBoard();
    }

    // Update is called once per frame
    void Update()
    {
        doInput();
    }

    private void OnDrawGizmos()
    {
        calculateExtents();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(extentWidth, extentHeight, 1));
    }

    void drawBoard()
    {
        calculateExtents();
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                dropGel(x, y);
            }
        }
    }

    void calculateExtents()
    {
        extentWidth = (Padding * (GelColumns + 1)) + (GelScale.x * GelColumns);
        extentHeight = (Padding * (GelRows + 1)) + (GelScale.y * GelRows);
        extentLeft = transform.position.x - (extentWidth / 2.0f);
        extentTop = transform.position.y - (extentHeight / 2.0f);
        gelZeroX = extentLeft + Padding + (GelScale.x / 2.0f);
        gelZeroY = extentTop + Padding + (GelScale.y / 2.0f);
        offY = extentTop - Camera.main.orthographicSize;
    }

    GameObject pick()
    {
        return GelPrefabs[Random.Range(0, GelPrefabs.Length)];
    }

    gel dropGel(int x, int y)
    {
        float u, v;
        u = gelZeroX + (x * (Padding + GelScale.x));
        v = gelZeroY + (y * (Padding + GelScale.y)) - offY;
        gel g = Instantiate(pick(), new Vector3(u, v, 0), Quaternion.identity).GetComponent<gel>();
        g.Init(x, y);
        g.transform.DOMoveY(v + offY, TweenFallDuration)
            .SetEase(Ease.InOutQuad)
            .SetDelay(Random.Range(0, TweenRndColumn) + (y * TweenRndRow))
            .OnComplete(() => { g.IsTouchable = true; });
        return g;
    }

    void doInput()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //TODO: Implement touch input
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                doTouchBegin(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                doTouchEnd(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                doTouchMoved(Input.mousePosition);
            }
        }
    }

    void doTouchBegin(Vector2 pos)
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        Collider2D hit = Physics2D.OverlapPoint(touchPos);
        if (hit)
        {
            touchGel = hit.transform.GetComponent<gel>();
            touchStart = touchPos;
            touchTracking = true;
            Debug.Log(string.Format("hit ({0},{1})", touchGel.Col, touchGel.Row));
        }
    }

    void doTouchMoved(Vector2 pos)
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wp.x, wp.y);

        if (touchTracking)
        {
            float dist = (touchStart - touchPos).magnitude;
            if (dist > DragDistance)
            {
                touchTracking = false;
                swipeDir = calculateDir(touchStart, touchPos);
                Debug.Log("swipeDir " + swipeDir);
            }
        }
    }

    void doTouchEnd(Vector2 pos)
    {
        touchTracking = false;
    }

    Vector2 calculateDir(Vector2 a, Vector2 b)
    {
        Vector2 result = Vector2.zero;

        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

        if (angle < 0)
            angle += 360;

        if (angle >= (90f - SwipeAngleLimit) && angle <= (90f + SwipeAngleLimit))
            result.y = 1f;
        else if (angle >= (270f - SwipeAngleLimit) && angle <= (270f + SwipeAngleLimit))
            result.y = -1f;
        else if (angle >= (180f - SwipeAngleLimit) && angle <= (180f + SwipeAngleLimit))
            result.x = -1f;
        else if (angle >= (360 - SwipeAngleLimit) && angle <= 360)
            result.x = 1f;
        else if (angle >= 0 && angle <= SwipeAngleLimit)
            result.x = 1f;

        return result;
    }
}
