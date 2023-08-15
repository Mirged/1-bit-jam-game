using JadePhoenix.Tools;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PaintSplatter : PoolableObject
{
    protected static int _currentMaxOrder = int.MinValue;
    protected SpriteRenderer _spriteRenderer;

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _currentMaxOrder = Mathf.Clamp(_currentMaxOrder + 1, int.MinValue, int.MaxValue);
        _spriteRenderer.sortingOrder = _currentMaxOrder;
    }
}
