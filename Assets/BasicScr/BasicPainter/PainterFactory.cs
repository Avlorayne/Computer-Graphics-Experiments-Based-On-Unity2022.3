// 你的原代码完全不变，仅添加这个工厂类

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public static class PainterFactory
{
    // 高性能缓存：Type -> 创建委托
    private static readonly Dictionary<Type, Func<Painter>> painterCreators = new();

    // 自动扫描所有Painter子类并注册
    public static void AutoRegisterAllPainters()
    {
        var painterTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(Painter).IsAssignableFrom(type) 
                         && !type.IsAbstract 
                         && type != typeof(Painter));

        foreach (var type in painterTypes)
        {
            RegisterPainter(type);
        }
        
        Debug.Log($"[PainterFactory.AutoRegisterAllPainters]\nAuto-registered {painterCreators.Count} painters");
    }

    // 手动注册（推荐用于特定顺序或参数）
    public static void RegisterPainter<T>() where T : Painter, new()
    {
        RegisterPainter(typeof(T));
    }

    public static void RegisterPainter(Type painterType)
    {
        if (painterType == null || !typeof(Painter).IsAssignableFrom(painterType) || painterType.IsAbstract)
            throw new ArgumentException($"Invalid painter type: {painterType?.Name}");

        // 编译为委托，性能比反射快10倍以上
        var constructor = painterType.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
            throw new InvalidOperationException($"{painterType.Name} must have a public parameterless constructor");

        var lambda = Expression.Lambda<Func<Painter>>(
            Expression.Convert(Expression.New(constructor), typeof(Painter))
        ).Compile();

        painterCreators[painterType] = lambda;
        Debug.Log($"[PainterFactory.RegisterPainter]\nRegistered: {painterType.Name}");
    }

    // 核心创建方法
    public static Painter CreatePainter(Type painterType)
    {
        if (painterType == null)
            throw new ArgumentNullException(nameof(painterType), "PainterType cannot be null!");

        // 如果未注册，尝试自动注册（仅一次）
        if (!painterCreators.TryGetValue(painterType, out var creator))
        {
            Debug.LogWarning($"[PainterFactory.CreatePainter]\nOn-demand registration for {painterType.Name}");
            RegisterPainter(painterType);
            creator = painterCreators[painterType];
        }

        var painter = creator();
        
        // 重置状态确保干净
        painter.PaintColor = Color.black;
        painter.BrushSize = 1;
        
        return painter;
    }

    // 泛型版本（类型安全）
    public static T CreatePainter<T>() where T : Painter, new()
    {
        return (T)CreatePainter(typeof(T));
    }
}
