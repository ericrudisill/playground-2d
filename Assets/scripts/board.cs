using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class board : MonoBehaviour
{

    public GameObject[] GelPrefabs;
    public int GelColumns = 7;
    public int GelRows = 8;
    public float GelScale = 1.0f;
    public float Padding = 0.15f;
    public float TweenFallDuration = 0.20f;
    public float TweenSwapDuration = 0.1f;
    public float TweenRndColumn = 0.1f;
    public float TweenRndRow = 0.05f;
    public float GravityDelay = 0.2f;
    public float DragDistance = 1.0f;
    public float SwipeAngleLimit = 30.0f;

    private Rect extents = Rect.zero;
    private float gelZeroX = 0;
    private float gelZeroY = 0;
    private float gelStep = 0;
    private float offY = 0;
    private Vector2 swipeDir = Vector2.zero;

    private gel touchGel = null;
    private gel touchPartner = null;
    private Vector2 touchStart = Vector2.zero;
    private bool touchTracking = false;

    private Vector2 DIR_NORTH = new Vector2(0, 1);
    private Vector2 DIR_SOUTH = new Vector2(0, -1);
    private Vector2 DIR_WEST = new Vector2(-1, 0);
    private Vector2 DIR_EAST = new Vector2(1, 0);

    private bool isTouchable = true;

    // Use this for initialization
    void Start()
    {
        drawBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTouchable)
        {
            doInput();

            if (swipeDir != Vector2.zero)
                doSwipe();
        }
    }

    private void OnDrawGizmos()
    {
        calculateExtents();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(extents.width, extents.height, 1));
    }

    void drawBoard()
    {
        calculateExtents();
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                gel g = dropGel(x, y);
            }
        }
    }

    void calculateExtents()
    {
        float extentWidth = (Padding * (GelColumns + 1)) + (GelScale * GelColumns);
        float extentHeight = (Padding * (GelRows + 1)) + (GelScale * GelRows);
        float extentLeft = transform.position.x - (extentWidth / 2.0f);
        float extentTop = transform.position.y - (extentHeight / 2.0f);
        extents = new Rect(extentLeft, extentTop, extentWidth, extentHeight);
        gelZeroX = extentLeft + Padding + (GelScale / 2.0f);
        gelZeroY = extentTop + Padding + (GelScale / 2.0f);
        gelStep = Padding + GelScale;
        offY = extentTop - Camera.main.orthographicSize;
    }

    GameObject pick(float x, float y)
    {
        List<GameObject> deck = new List<GameObject>(GelPrefabs);
        List<GameObject> neighbors = getNeighbors(x, y);
        foreach (GameObject g in neighbors)
            deck.Remove(deck.Find(f => g.name.StartsWith(f.name)));
        return deck[Random.Range(0, deck.Count)];
    }

    List<GameObject> getNeighbors(float x, float y)
    {
        List<GameObject> neighbors = new List<GameObject>();
        Vector2 center = new Vector2(x, y);
        GameObject g;

        g = getNeighbor(center, DIR_NORTH); if (g != null) neighbors.Add(g);
        g = getNeighbor(center, DIR_SOUTH); if (g != null) neighbors.Add(g);
        g = getNeighbor(center, DIR_EAST); if (g != null) neighbors.Add(g);
        g = getNeighbor(center, DIR_WEST); if (g != null) neighbors.Add(g);

        return neighbors;
    }

    GameObject getNeighbor(Vector2 pos, Vector2 dir)
    {
        Vector2 target = pos + (dir * gelStep);
        Collider2D hit = Physics2D.OverlapPoint(target);
        if (hit) return hit.transform.gameObject;
        return null;
    }

    gel getGel(int x, int y, float offsetY = 0)
    {
        float u, v;
        u = gelZeroX + (x * (Padding + GelScale));
        v = gelZeroY + (y * (Padding + GelScale)) + offsetY;
        Collider2D hit = Physics2D.OverlapPoint(new Vector2(u, v));
        if (hit) return hit.gameObject.GetComponent<gel>();
        return null;
    }

    gel dropGel(int x, int y)
    {
        float u, v;
        u = gelZeroX + (x * (Padding + GelScale));
        v = gelZeroY + (y * (Padding + GelScale)) - offY;
        gel g = Instantiate(pick(u, v), new Vector3(u, v, 0), Quaternion.identity).GetComponent<gel>();
        g.transform.DOMoveY(v + offY, TweenFallDuration)
            .SetEase(Ease.InOutQuad)
            .SetDelay(Random.Range(0, TweenRndColumn) + (y * TweenRndRow))
            .OnComplete(() => { g.IsTouchable = true; });
        return g;
    }

    void doInput()
    {
        swipeDir = Vector2.zero;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
			if (Input.touchCount > 0)
			{
				Touch t = Input.GetTouch (0);
				if (t.phase == TouchPhase.Began) {
					doTouchBegin (t.position);
				}
				if (t.phase == TouchPhase.Ended) {
					doTouchEnd (t.position);
				}
				if (t.phase == TouchPhase.Moved) {
					doTouchMoved (t.position);
				}
			}
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

    void doSwipe()
    {
        // Raycast will hit self, so temporarily turn off gel's collider
        Collider2D c = touchGel.GetComponent<Collider2D>();
        c.enabled = false;
        RaycastHit2D hit = Physics2D.Raycast(touchGel.transform.position, swipeDir);
        c.enabled = true;

        if (hit)
        {
            touchPartner = hit.collider.transform.GetComponent<gel>();
            Vector2 touchGelOld = new Vector2(touchGel.transform.position.x, touchGel.transform.position.y);
            Vector2 touchPartnerOld = new Vector2(touchPartner.transform.position.x, touchPartner.transform.position.y);
            touchGel.transform.DOMove(hit.collider.transform.position, TweenSwapDuration);
            touchPartner.transform.DOMove(touchGel.transform.position, TweenSwapDuration)
                .OnComplete(() =>
                {
                    if (isGoodSwap(touchGel, touchPartner) == false)
                    {
                        touchGel.transform.DOMove(touchGelOld, TweenSwapDuration);
                        touchPartner.transform.DOMove(touchPartnerOld, TweenSwapDuration);
                    }
                    else
                    {
                        StartCoroutine(doBoard());
                    }
                });
        }
    }

    bool isGoodSwap(gel a, gel b)
    {
        // 2 North, checking south
        if (hasMatchOnPath(a, calcRelPos(a, DIR_NORTH, 2), DIR_SOUTH, 5)) return true;
        if (hasMatchOnPath(b, calcRelPos(b, DIR_NORTH, 2), DIR_SOUTH, 5)) return true;

        // 2 South, checking north
        if (hasMatchOnPath(a, calcRelPos(a, DIR_SOUTH, 2), DIR_NORTH, 5)) return true;
        if (hasMatchOnPath(b, calcRelPos(b, DIR_SOUTH, 2), DIR_NORTH, 5)) return true;

        // 2 West, checking east
        if (hasMatchOnPath(a, calcRelPos(a, DIR_WEST, 2), DIR_EAST, 5)) return true;
        if (hasMatchOnPath(b, calcRelPos(b, DIR_WEST, 2), DIR_EAST, 5)) return true;

        // 2 East, checking west
        if (hasMatchOnPath(a, calcRelPos(a, DIR_EAST, 2), DIR_WEST, 5)) return true;
        if (hasMatchOnPath(b, calcRelPos(b, DIR_EAST, 2), DIR_WEST, 5)) return true;

        return false;
    }

    Vector2 calcRelPos(gel g, Vector2 dir, int hops)
    {
        return (Vector2)g.transform.position + (dir * (hops * gelStep));

    }

    bool hasMatchOnPath(gel g, Vector2 pos, Vector2 dir, int limit)
    {
        float dist = limit * gelStep;
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, dir, dist);

        // Shortcut - impossible to have match 3 with fewer hits (probably went off the board)
        if (hits.Length < 3) return false;

        int count = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.name == g.gameObject.name)
                count++;
            else
                count = 0;

            if (count == 3)
                return true;
        }
        return false;
    }

    bool markMatches()
    {
        bool hasMatches = false;
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                gel g = getGel(x, y);
                if (g)
                {
                    hasMatches |= markMatchesOnPath(g, DIR_EAST);
                    hasMatches |= markMatchesOnPath(g, DIR_NORTH);
                }
            }
        }
        return hasMatches;
    }

    void markMatchNeighbors()
    {
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                gel g = getGel(x, y);
                if (g != null && g.IsMatched)
                {
                    List<GameObject> neighbors = getNeighbors(g.transform.position.x, g.transform.position.y);
                    foreach (GameObject n in neighbors)
                    {
                        if (n.name == g.gameObject.name)
                        {
                            n.GetComponent<gel>().SetMatched();
                        }
                    }
                }
            }
        }
    }

    bool markMatchesOnPath(gel g, Vector2 dir)
    {
        // Search two additonal gels in dir (self + 2 = 3)
        float dist = 2 * gelStep;
        RaycastHit2D[] hits = Physics2D.RaycastAll(g.transform.position, dir, dist);

        // Shortcut - impossible to have match 3 with fewer hits (probably went off the board)
        if (hits.Length < 3) return false;

        // We know hit 0 is the parameter g, so just check hit 1 and 2
        if (hits[1].transform.gameObject.name == g.gameObject.name && hits[2].transform.gameObject.name == g.gameObject.name)
        {
            g.SetMatched();
            hits[1].transform.GetComponent<gel>().SetMatched();
            hits[2].transform.GetComponent<gel>().SetMatched();
            return true;
        }

        return false;
    }

    void processMatches()
    {
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                gel g = getGel(x, y);
                if (g != null && g.IsMatched)
                {
                    g.Pop();
					// Apply score: score += GelScore
                }
            }
        }
    }

    IEnumerator applyGravity()
    {
        // For each column, walk north until empty.
        // Increment for each empty until non-empty found
        // Set non-empty to fall south empty-count cells, calculate duration accordingly
        // Keep walking north until finished

        yield return new WaitForSeconds(GravityDelay);

        float maxDuration = 0;
        for (int x = 0; x < GelColumns; x++)
        {
            int emptyCount = 0;
            for (int y = 0; y < GelRows; y++)
            {
                gel g = getGel(x, y);
                if (g == null)
                {
                    emptyCount++;
                }
                else
                {
                    float newY = g.transform.position.y - (emptyCount * gelStep);
                    float duration = TweenSwapDuration * emptyCount;
                    if (duration > maxDuration) maxDuration = duration;
                    g.transform.DOMoveY(newY, duration);
                }
            }
        }

        yield return new WaitForSeconds(maxDuration);
    }

    IEnumerator doBoard()
    {
        isTouchable = false;
        while (markMatches())
        {
            markMatchNeighbors();
            processMatches();
            yield return applyGravity();
        }
        if (stillHasSwaps() == false)
        {
            yield return fillBoard();
        }
        isTouchable = true;
    }

    IEnumerator fillBoard()
    {
        // Move the board off screen so dropGel works properly
        GameObject[] gels = GameObject.FindGameObjectsWithTag("Gel");
        foreach (GameObject go in gels)
        {
            go.transform.Translate(0, -offY, 0);
        }

        // Fill the board like normal
        for (int y=0;y<GelRows;y++)
        {
            for (int x=0;x<GelColumns;x++)
            {
                gel g = getGel(x, y, -offY);
                if (g == null)
                    dropGel(x, y);  //TODO: Pick evaluates -offY, so it doesn't work right. Find alternative.
            }
        }

        // Move the original board back
        foreach (GameObject go in gels)
        {
            go.transform.Translate(0, offY, 0);
        }
        yield return new WaitForSeconds(TweenFallDuration);
    }

    bool stillHasSwaps()
    {
        gel candidate1 = null, candidate2 = null;
        for (int y = 0; y < GelRows; y++)
        {
            for (int x = 0; x < GelColumns; x++)
            {
                gel[,] kernel = getKernel(x, y);
                if (kernel != null)
                {
                    // Try to swap East
                    if (tryKernelSwap(kernel, 2, 1))
                    {
                        if (kernalHasMatch(kernel))
                        {
                            candidate1 = kernel[1, 1];
                            candidate2 = kernel[1, 2];
                        }
                        // Swap back
                        tryKernelSwap(kernel, 2, 1);
                    }
                    // Try to swap North
                    if (tryKernelSwap(kernel, 1, 2))
                    {
                        if (kernalHasMatch(kernel))
                        {
                            candidate1 = kernel[1, 1];
                            candidate2 = kernel[2, 1];
                        }
                    }
                }
            }
        }
        if (candidate1 != null)
        {
            Debug.Log("Still has swaps!");
            return true;
        }
        else
        {
            Debug.Log("NO SWAPS LEFT!!!");
            return false;
        }
    }

    bool kernalHasMatch(gel[,] kernel)
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (gelCompare(kernel[y, x], kernel[y + 1, x]) && gelCompare(kernel[y, x], kernel[y + 2, x]))
                {
                    return true;
                }
                if (gelCompare(kernel[y, x], kernel[y, x + 1]) && gelCompare(kernel[y, x], kernel[y, x + 2]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool gelCompare(gel a, gel b)
    {
        if (a == null || b == null)
            return false;
        if (a.name != b.name)
            return false;
        return true;
    }

    /**
     * Swaps center element with element at targetX/Y. 
     * Returns false if target is null
     **/
    bool tryKernelSwap(gel[,] kernel, int targetX, int targetY)
    {
        gel target = kernel[targetY, targetX];
        if (target == null)
            return false;
        kernel[targetY, targetX] = kernel[1, 1];
        kernel[1, 1] = target;
        return true;
    }

    gel[,] getKernel(int x, int y)
    {
        gel[,] kernel = new gel[5, 5];
        gel center = getGel(x, y);
        if (center == null)
            return null;
        int j = 0, k = 0;
        for (int v = y - 2; v <= y + 2; v++)
        {
            k = 0;
            for (int u = x - 2; u <= x + 2; u++)
            {
                // Calling getGel is a bit inefficient, but gets the job done
                gel g = getGel(u, v);
                if (g != null)
                    kernel[j, k] = g;
                k++;
            }
            j++;
        }
        return kernel;
    }
}
