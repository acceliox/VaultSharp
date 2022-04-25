// Licensed to acceliox GmbH under one or more agreements.
// See the LICENSE file in the project root for more information.Copyright (c) acceliox GmbH. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VaultSharp.Samples
{
    public static class VaultExtensions
    {
        public static T? ToType<T>(this IDictionary<string, object> dict, string objName)
        {
            dict.TryGetValue(objName, out var obj);
            var objInJson = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(objInJson);
        }

        public static object GetField<T>(this T obj, string fieldName)
        {
            var type = typeof(T);
            return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static async Task<object> InvokeMethod<T>(this T obj, string methodName, params object[] args)
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return await (Task<object>) method.Invoke(obj, args);
        }

        public static async Task<object> InvokeMethodWithType<T>(this T obj, Type type, string methodName,
            params object[] args)
        {
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return await (Task<object>) method.Invoke(obj, args);
        }
    }
}