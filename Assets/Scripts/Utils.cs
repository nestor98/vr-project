using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public static class Utils
{


    // Doesnt allow to go beyond +90 and -90
    private static float clampLatitude(float euler)
    {
        if (euler > 180.0f) return Mathf.Clamp(euler, 270.0f, 360.0f);
        else return Mathf.Min(euler, 90.0f);
    }

    public static Vector3 CapEulerAngles(Vector3 euler)
    {
        return new Vector3(clampLatitude(euler.x), 
                           euler.y, euler.z);
    }


    //public static T AddComponent<T>(this GameObject game, T duplicate) where T : Component
    //{
    //    T target = game.AddComponent<T>();
    //    foreach (PropertyInfo x in typeof(T).GetProperties())
    //        if (x.CanWrite)
    //            x.SetValue(target, x.GetValue(duplicate));
    //    return target;
    //}

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

     public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }



};
