using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using System.Reflection;namespace CustomExtensions{    /// <summary>
    /// Extension methods that may be used everywhere.
    /// </summary>    public static partial class General    {        /// <summary>
        /// Determines if target object is null OR empty (as an ICollection)
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T Source) where T : class
        {
            return Source == null ? true : (Source as ICollection) == null ? false : (Source as ICollection).Count <= 0;
        }

        /// <summary>
        /// If a particular element in the source IEnumerable meets the specified condition then perform a specified action(s).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Condition"></param>
        /// <param name="Action"></param>
        public static void ConditionalAction<T>(this IEnumerable<T> Source, Predicate<T> Condition, params Action<T>[] Actions)
        {
            if (Source.IsNullOrEmpty())
                throw new ArgumentNullException();

            foreach (T Item in Source)
                if (Condition(Item))
                    foreach (Action<T> Action in Actions)
                        Action(Item);
        }        /// <summary>
        /// Resizes a target list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List"></param>
        /// <param name="NewCapacity"></param>
        /// <param name="NewObjects"></param>        public static void Resize<T>(this List<T> List, int NewCapacity, T NewObjects = default(T))
        {
            int OldCapacity = List.Count;

            if (OldCapacity > NewCapacity)
                List.RemoveRange(NewCapacity, OldCapacity - NewCapacity);
            else
                List.AddRange(Enumerable.Repeat(NewObjects, NewCapacity - OldCapacity));
        }        /// <summary>
        /// If it can it will convert a target list into another type.
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Converter"></param>
        /// <returns></returns>        public static List<TOutput> ConvertValid<TInput, TOutput>(this List<TInput> Source, Converter<TInput, TOutput> Converter) where TOutput: class where TInput: class        {
            List<TOutput> New = new List<TOutput>();            foreach(TInput Obj in Source)
                if(!Converter(Obj).IsNullOrEmpty())
                    New.Add(Converter(Obj));

            return New;
        }        /// <summary>
        /// Returns a random element of a list. If empty it returns the default value of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <returns></returns>        public static T Random<T>(this List<T> Source)
        {
            if (Source.IsNullOrEmpty())
                return default(T);
            else
                return Source[UnityEngine.Random.Range(0, Source.Count)];
        }        /// <summary>
        /// Same as .NET FirstOrDefault but allows specification of default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Predicate"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>        public static T FirstOrDefault<T>(this IEnumerable<T> Source, Func<T, bool> Predicate, T DefaultValue = default(T))
        {
            if (Source.IsNullOrEmpty()) return DefaultValue;

            foreach (T Obj in Source)
                if (Predicate(Obj))
                    return Obj;

            return DefaultValue;
        }        /// <summary>
        /// Returns itself if the predicate is false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Predicate"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>        public static T Default<T>(this T Source, Func<T, bool> Predicate, T DefaultValue)
        {
            if (Predicate(Source))
                return DefaultValue;
            else
                return Source;
        }    }    /// <summary>
    /// Unity specific extension methods.
    /// </summary>    public static partial class Unity
    {
        /// <summary>
        /// Returns the interface object if any components in the source game object implement it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="IncludeChildren"></param>
        /// <returns></returns>
        public static T GetInterface<T>(this UnityEngine.GameObject Source, bool IncludeParents = false, bool IncludeChildren = false, bool Infallible = false) where T : class
        {
            // Null checking
            if (Source.IsNullOrEmpty())
                if (Infallible)
                    return default(T);
                else
                    throw new ArgumentNullException();

            // T must be an interface
            if (!typeof(T).IsInterface)
                if (Infallible)
                    return default(T);
                else
                    throw new ArgumentException("T Must be an interface.");

            // List of components on itself, in children and parent (gets monobehaviours instead because this method specializes in finding custom interfaces)
            List<MonoBehaviour> Components = Source.GetComponents<MonoBehaviour>().ToList();

            // Include parents and children if specified
            if (IncludeParents) Components = Components.Concat(Source.GetComponentsInParent<MonoBehaviour>()).ToList();
            if (IncludeChildren) Components = Components.Concat(Source.GetComponentsInChildren<MonoBehaviour>()).ToList();

            // Attempt to return interface
            T InterfaceObj = Components.FirstOrDefault(x => x is T) as T;
            return !InterfaceObj.IsNullOrEmpty() ? InterfaceObj : default(T);
        }

        /// <summary>
        /// Copies over field and property values from one component to another (WARNING: uses reflection)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Other"></param>
        /// <returns>The source object with newly changed properties and fields.</returns>
        public static T GetCopyof<T>(this Component Source, T Other) where T : Component
        {
            if (Source.IsNullOrEmpty() || Other.IsNullOrEmpty())
                throw new ArgumentNullException();

            Type Type = Source.GetType();

            // Make sure each type is the same
            if (Type != Other.GetType())
                throw new ArgumentException("Other must be the same type as the source.");

            // Using reflection get the type's properties and fields
            BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] PropertyMembers = Type.GetProperties(Flags);
            FieldInfo[] FieldMembers = Type.GetFields(Flags);

            // Set the property values
            foreach (var PropertyMember in PropertyMembers)
            {
                // Check if the property is writable and not obsolete
                if (PropertyMember.CanWrite && !PropertyMember.IsDefined(typeof(ObsoleteAttribute), true))
                {
                    try { PropertyMember.SetValue(Source, PropertyMember.GetValue(Other, null), null); }
                    catch { } // Just in case of an exception throw
                }
            }

            // Set the field values
            foreach (var FieldMemeber in FieldMembers)
            {
                // Again make sure the field is not obsolete
                if (!FieldMemeber.IsDefined(typeof(ObsoleteAttribute), true))
                    FieldMemeber.SetValue(Source, FieldMemeber.GetValue(Other));
            }

            return Source as T;
        }

        /// <summary>
        /// Clones an existing component and adds it onto target game object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="GameObj"></param>
        /// <param name="CompToAdd"></param>
        /// <returns></returns>
        public static T AddComponent<T>(this GameObject GameObj, T CompToAdd) where T : Component
        {
            return GameObj.AddComponent<T>().GetCopyof<T>(CompToAdd) as T;
        }

        /// <summary>
        /// Plays an audio clip and audio source. Handles null-checking. Can also specify if the audio source should be detached.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Clip"></param>
        /// <param name="Detatch"></param>
        /// <returns>True if the clip was successfully played on the source.</returns>
        public static bool PlayClip(this AudioSource Source, AudioClip Clip, bool Detach = true)
        {
            // Null checking
            if (Source.IsNullOrEmpty() || Clip.IsNullOrEmpty() || Source == null || Clip == null)
                return false;

            if (!Detach) // Play the clip on the source normally.
            {
                Source.clip = Clip;
                Source.time = 0;
                Source.Play();
                return true;
            }
            else // Create a new game object, copy over the audio source component, play the clip on new audio source, then destroy object.
            {
                // Create and name new game object
                GameObject NewObject = new GameObject();
                NewObject.name = "Detached Sound Object (" + Source.gameObject.name + ")";

                // Add a new audio source component to the new game object and copy over values from the source audio component.
                AudioSource NewSource;
                try { NewSource = NewObject.AddComponent<AudioSource>(Source); }
                catch { return false; }

                // Destroy the new game object after the clip has finished playing
                GameObject.Destroy(NewObject, Clip.length + 0.1f);

                // Play the clip on the new audio source
                return NewSource.PlayClip(Clip, false);
            }
        }

        /// <summary>
        /// Returns a vector 2 from a vector 3 ignoring the Y axis.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static Vector2 IgnoreY2(this Vector3 Source)
        {
            return new Vector2(Source.x, Source.z);
        }

        /// <summary>
        /// Zeros a vector 3's Y value.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static Vector3 IgnoreY3(this Vector3 Source, float defaultY = 0)
        {
            return new Vector3(Source.x, defaultY, Source.z);
        }

        /// <summary>
        /// Returns the length of the animation curve in seconds.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static float Duration(this AnimationCurve Source)
        {
            if (Source.IsNullOrEmpty() || Source.length <= 0)
                throw new NullReferenceException();

            return Source[Source.length - 1].time;
        }

        /// <summary>
        /// Returns a component from an array of ray cast hit.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static T GetComponent<T>(this RaycastHit[] Source) where T: Component
        {
            foreach (var Hit in Source)
            {
                if (Hit.collider.GetComponent<T>())
                    return Hit.collider.GetComponent<T>();
            }
            return default(T);
        }

        /// <summary>
        /// Instantiates a new instance of a component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Original"></param>
        /// <param name="Position"></param>
        /// <param name="Rotation"></param>
        /// <param name="Parent"></param>
        /// <returns></returns>
        public static T Instantiate<T>(this T Original, Transform Parent = null) where T: Component
        {
            if (Original.IsNullOrEmpty()) throw new NullReferenceException();

            T NewComponent = (T)GameObject.Instantiate(Original);

            if(Parent) NewComponent.transform.parent = Parent;

            return NewComponent;
        }
    }}