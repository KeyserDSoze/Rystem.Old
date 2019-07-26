using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public static class DeepCopyExtension
    {
        public static dynamic DeepCopy(this object original, params object[] args)
        {
            return InternalCopy(original, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }
        private static Object InternalCopy(object originalObject, IDictionary<Object, Object> visited, params object[] args)
        {
            if (originalObject == null)
                return null;
            Type typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect))
                return originalObject;
            if (visited.ContainsKey(originalObject))
                return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect))
                return null;
            object cloneObject = args.Length == 0 ? CloneMethod.Invoke(originalObject, null) : Activator.CreateInstance(typeToReflect, args);
            if (typeof(IList).IsAssignableFrom(typeToReflect))
            {
                IList clonedArray = (IList)Activator.CreateInstance(typeToReflect);
                foreach (dynamic singleValue in (IList)originalObject)
                    clonedArray.Add(InternalCopy(singleValue, visited));
            }
            else if (typeof(IDictionary).IsAssignableFrom(typeToReflect))
            {
                IDictionary clonedArray = (IDictionary)Activator.CreateInstance(typeToReflect);
                foreach (dynamic singleValue in (IDictionary)originalObject)
                    clonedArray.Add(InternalCopy(singleValue.Key, visited), InternalCopy(singleValue.Value, visited));
            }
            else if (typeof(IEnumerable).IsAssignableFrom(typeToReflect) && typeToReflect.GetElementType() != null)
            {
                Type type = typeToReflect.GetElementType();
                Array clonedArray = Array.CreateInstance(type, ((Array)originalObject).Length);
                int counting = 0;
                foreach (dynamic singleValue in (Array)originalObject)
                    clonedArray.SetValue(InternalCopy(singleValue, visited), counting++);
            }
            else
            {
                visited.Add(originalObject, cloneObject);
                CopyFields(originalObject, visited, cloneObject, typeToReflect);
                CopyProperties(originalObject, visited, cloneObject, typeToReflect);
            }
            return cloneObject;
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (IsPrimitive(fieldInfo.FieldType))
                    continue;
                object originalFieldValue = fieldInfo.GetValue(originalObject);
                object clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        private static void CopyProperties(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
        {

            foreach (PropertyInfo propertyInfo in typeToReflect.GetProperties(bindingFlags))
            {
                try
                {
                    if (IsPrimitive(propertyInfo.PropertyType))
                        continue;
                    if (propertyInfo.SetMethod == null)
                        continue;
                    object originalFieldValue = propertyInfo.GetValue(originalObject);
                    object clonedFieldValue = InternalCopy(originalFieldValue, visited);
                    propertyInfo.SetValue(cloneObject, clonedFieldValue);
                }
                catch (Exception er)
                {
                    string dj = er.ToString();
                }
            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }
    public static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }
    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }
        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
