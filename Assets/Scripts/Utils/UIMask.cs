using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TestGame.Utils;
using UnityEngine;

public class UIMask : MonoBehaviour
{
    [SerializeField] private GameObject maskItem;

    private Stack<GameObject> maskItems = new();
    private Queue<GameObject> maskItemPool = new();
    private Queue<Vector2> targetItems = new();
    private bool isMove = false;

    public void FromTo(Vector2 to)
    {
        if (isMove)
        {
            targetItems.Enqueue(to);
            return;
        }

        MoveNext(to);
    }

    private void MoveNext(Vector2 to)
    {
        if (maskItems.Count == 0)
        {
            maskItems.Push(GetItem(to));
            return;
        }

        Vector2 from = maskItems.Peek().transform.position;
        isMove = true;
        DOTween.To(() => from, vec2 =>
        {
            from = vec2;
            maskItems.Push(GetItem(from));
        }, to, 0.5f).onComplete = () =>
        {
            if (targetItems.Count > 0)
            {
                MoveNext(targetItems.Dequeue());
            }
            else
            {
                isMove = false;
            }
        };
    }

    public void ClearTo(Vector2 to)
    {
        StartCoroutine(ClearTween(to));
    }

    IEnumerator ClearTween(Vector2 to)
    {
        while (maskItems.Count > 0)
        {
            var item = maskItems.Peek();
            bool finish = UIUtil.AreVectorsEqual(item.transform.position, to);
            if (finish)
            {
                yield break;
            }
            RecycleItem(maskItems.Pop());
            yield return null;
        }
    }

    void RecycleItem(GameObject item)
    {
        item.transform.position = Vector3.one * 1000;
        maskItemPool.Enqueue(item);
    }

    GameObject GetItem(Vector2 position)
    {
        if (maskItemPool.Count == 0)
        {
            return Instantiate(maskItem, position, Quaternion.identity, transform);
        }
        else
        {
            var item = maskItemPool.Dequeue();
            item.transform.position = position;
            return item;
        }
    }
}