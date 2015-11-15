using UnityEngine;using System.Collections;using System.Collections.Generic;using System.Linq;
using System;
using System.Reflection;
using System.Diagnostics;namespace CustomExtensions{    /// <summary>
    /// Extension methods that may be used everywhere.
    /// </summary>    public static partial class General    {        /// <summary>
        /// Determines if target object is null OR empty (as an ICollection)
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T Source)
        {
            if (Source == null)
                return true;
            else if (typeof(T).IsValueType)
                return false;
            else if ((Source as ICollection) != null)
                return (Source as ICollection).Count <= 0;
#pragma warning disable 168
            else if ((Source as IEnumerable) != null)
            {
                int count = 0;
                foreach (var elem in (Source as IEnumerable))
                    count++;
                return count <= 0;
            }
#pragma warning restore 168
            else
                return false;
        }

        /// <summary>
        /// If a particular element in the source IEnumerable meets the specified condition then perform a specified action(s).
        /// </summary>
        public static void ConditionalAction<T>(this IEnumerable<T> Source, Predicate<T> Condition, Action<T> Action)
        {
            if (Source.IsNullOrEmpty())
                throw new ArgumentNullException();

            foreach (T Item in Source)
                if (Condition(Item))
                    Action(Item);
        }        /// <summary>
        /// Retrives an element safely from a list. Checks for null and appropriate index.
        /// </summary>        public static T SafeGet<T>(this IList<T> Source, int Index) where T: class
        {
            if (Source.IsNullOrEmpty()) return null;
            if (Index < 0 || Index >= Source.Count) return null;
            return Source[Index];
        }        /// <summary>
        /// Resizes a target list.
        /// </summary>        public static void Resize<T>(this List<T> List, int NewCapacity, T NewObjects = default(T))
        {
            int OldCapacity = List.Count;

            if (OldCapacity > NewCapacity)
                List.RemoveRange(NewCapacity, OldCapacity - NewCapacity);
            else
                List.AddRange(Enumerable.Repeat(NewObjects, NewCapacity - OldCapacity));
        }

        /// <summary>
        /// If it can it will convert a target IEnumerable into another type.
        /// </summary>
        public static IEnumerable<TOutput> ConvertValid<TInput, TOutput>(this IEnumerable<TInput> Source, Converter<TInput, TOutput> Converter)
        {
            foreach (TInput Obj in Source)
            {
                TOutput Temp = Converter(Obj);
                if (!(Temp as object).IsNullOrEmpty())
                    yield return Temp;
            }
        }        /// <summary>
        /// Returns a random element of a list. If empty it returns the default value of specified type.
        /// </summary>        public static T Random<T>(this List<T> Source)
        {
            if (Source.IsNullOrEmpty())
                return default(T);
            else
                return Source[UnityEngine.Random.Range(0, Source.Count)];
        }        /// <summary>
        /// Same as .NET FirstOrDefault but allows specification of default value.
        /// </summary>        public static T FirstOrDefault<T>(this IEnumerable<T> Source, Func<T, bool> Predicate, T DefaultValue)
        {
            if (Source.IsNullOrEmpty()) return DefaultValue;

            foreach (T Obj in Source)
                if (Predicate(Obj))
                    return Obj;

            return DefaultValue;
        }        /// <summary>
        /// Returns itself if the predicate is false.
        /// </summary>        public static T Default<T>(this T Source, Func<T, bool> Predicate, T DefaultValue)
        {
            if (Predicate(Source))
                return DefaultValue;
            else
                return Source;
        }        /// <summary>
        /// Normalizes a float between a new float range.
        /// </summary>        public static float Normalize(this float Source, float OldMin, float OldMax, float NewMin, float NewMax)
        {
            return Mathf.Lerp(NewMin, NewMax, (Source - OldMin) / (OldMax - OldMin));
        }

        /// <summary>
        /// Returns true if the source object equals any of the objects given.
        /// </summary>
        public static bool EqualToAny<T>(this T Source, params T[] Comparisons)
        {
            foreach (T Comparison in Comparisons)
                if (Source.Equals(Comparison))
                    return true;

            return false;
        }        /// <summary>
        /// Similar to FirstOrDefault except this includes a max number of iterations. Returns default if it reaches that point.
        /// </summary>        public static T FirstOrDefaultWithMax<T>(this List<T> Source, Func<T, bool> Predicate, int Max)
        {
            for (int i = 0; i < Source.Count; i++)
            {
                if (i >= Max) return default(T);
                if (Predicate(Source[i])) return Source[i];
            }

            return default(T);
        }

        /// <summary>
        /// Similar to FirstOrDefault except this includes a max number of iterations. Returns the last valid value or default if it reaches that point.
        /// </summary>        public static T LastOrDefaultWithMax<T>(this List<T> Source, Func<T, bool> Predicate, int Max)
        {
            for (int i = Source.Count - 1; i >= 0; i--)
            {
                if (i < Source.Count - Max) return default(T);
                if (Predicate(Source[i])) return Source[i];
            }

            return default(T);
        }

        /// <summary>
        /// Similar to FirstOrDefault except this only includes values in a range. Returns default if it reaches the end.
        /// </summary>
        public static T FirstOrDefaultInRange<T>(this List<T> Source, Func<T, bool> Predicate, int From, int To)
        {
            for (int i = Math.Max(0, From - 1); i < Source.Count; i++)
            {
                if (i >= To) return default(T);
                if (Predicate(Source[i])) return Source[i];
            }

            return default(T);
        }

        /// <summary>
        /// Similar to LastOrDefault except this only includes values in a range. Returns default if it reaches the start.
        /// </summary>
        public static T LastOrDefaultInRange<T>(this List<T> Source, Func<T, bool> Predicate, int From, int To)
        {
            for (int i = To; i >= 0; i--)
            {
                if (i < From - 1) return default(T);
                if (Predicate(Source[i])) return Source[i];
            }

            return default(T);
        }    }    /// <summary>
    /// Unity specific extension methods.
    /// </summary>    public static partial class Unity
    {
        /// <summary>
        /// Returns the interface object if any components in the source game object implement it.
        /// </summary>
        public static T GetInterface<T>(this UnityEngine.GameObject Source, bool IncludeParents = false, bool IncludeChildren = false, bool Infallible = false) where T : class, IUnityInterface
        {
            // Null checking
            if (Source.IsNullOrEmpty())
                if (Infallible)
                    return default(T);
                else
                    throw new ArgumentNullException();

            // List of components on itself, in children and parent (gets monobehaviours instead because this method specializes in finding custom interfaces)
            List<MonoBehaviour> Components = Source.GetComponents<MonoBehaviour>().ToList();

            // Include parents and children if specified
            if (IncludeParents) Components = Components.Concat(Source.GetComponentsInParent<MonoBehaviour>()).ToList();
            if (IncludeChildren) Components = Components.Concat(Source.GetComponentsInChildren<MonoBehaviour>()).ToList();

            // Attempt to return interface
            T InterfaceObj = Components.FirstOrDefaultWithMax(x => x is T, Components.Count) as T;
            return !InterfaceObj.IsNullOrEmpty() ? InterfaceObj : default(T);
        }

        /// <summary>
        /// Copies over field and property values from one component to another (WARNING: uses reflection)
        /// </summary>
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
        public static T AddComponent<T>(this GameObject GameObj, T CompToAdd) where T : Component
        {
            return GameObj.AddComponent<T>().GetCopyof<T>(CompToAdd) as T;
        }

        /// <summary>
        /// Plays an audio clip on the specified audio source. Handles null-checking.
        /// </summary>
        /// <returns>True if the clip was successfully played on the source.</returns>
        public static void PlayClipAttached(this AudioSource Source, AudioClip Clip)
        {
            // Null checking
            if (Source == null || Clip == null) return;

            Source.clip = Clip;
            Source.time = 0;
            Source.Play();
        }

        /// <summary>
        /// Plays an audio clip on a clone of the specified audio source. Handles null-checking.
        /// </summary>
        public static AudioSource PlayClipDetached(this AudioSource Source, AudioClip Clip, Transform Target = null)
        {
            // Null checking
            if (Source == null || Clip == null) return null;

            // Create a new game object, copy over the audio source component, play the clip on new audio source, then destroy object.
            // Create and name new game object
            GameObject NewObject = new GameObject();
            NewObject.transform.SetParent(Target);
            NewObject.hideFlags = HideFlags.HideInHierarchy;
            NewObject.name = "Sound Object (" + Source.gameObject.name + " - " + Clip.name + ")";

            // Add a new audio source component to the new game object and copy over values from the source audio component.
            AudioSource NewSource;
            try { NewSource = NewObject.AddComponent<AudioSource>(Source); }
            catch { return null; }

            // Destroy the new game object after the clip has finished playing
            GameObject.Destroy(NewObject, Clip.length + 0.1f);

            // Play the clip on the new audio source
            NewSource.PlayClipAttached(Clip);
            return NewSource;
        }

        /// <summary>
        /// Returns a vector 2 from a vector 3 ignoring the Y axis.
        /// </summary>
        public static Vector2 IgnoreY2(this Vector3 Source)
        {
            return new Vector2(Source.x, Source.z);
        }

        /// <summary>
        /// Zeros a vector 3's Y value.
        /// </summary>
        public static Vector3 IgnoreY3(this Vector3 Source, float defaultY = 0)
        {
            return new Vector3(Source.x, defaultY, Source.z);
        }

        /// <summary>
        /// Returns the length of the animation curve in seconds.
        /// </summary>
        public static float Duration(this AnimationCurve Source)
        {
            if (Source.IsNullOrEmpty() || Source.length <= 0)
                throw new NullReferenceException();

            return Source[Source.length - 1].time;
        }

        /// <summary>
        /// Returns a component from an array of ray cast hit.
        /// </summary>
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
        /// Instantiates a new instance of a component and can parent it to another object.
        /// </summary>
        public static T Instantiate<T>(this T Original, Transform Parent = null) where T: Component
        {
            if (Original.IsNullOrEmpty()) throw new NullReferenceException();

            T NewComponent = (T)GameObject.Instantiate(Original);

            if(Parent) NewComponent.transform.parent = Parent;

            return NewComponent;
        }

        /// <summary>
        /// Instantiates a new instance of a component, can specify its name and can parent it to another object.
        /// </summary>
        public static T Instantiate<T>(this T Original, Transform Parent, string Name) where T : Component
        {
            if (Original.IsNullOrEmpty()) throw new NullReferenceException();

            T NewComponent = (T)GameObject.Instantiate(Original);

            if (Parent) NewComponent.transform.parent = Parent;

            NewComponent.name = Name;

            return NewComponent;
        }

        /// <summary>
        /// Iterates through both game object colliders and makes them ignore eachother.
        /// </summary>
        public static void IgnoreCollision(this GameObject Source, GameObject Other, bool Ignore = true)
        {
            if (!Source || !Other) return;

            foreach (var SCol in Source.GetComponentsInChildren<Collider>())
            {
                foreach (var OCol in Other.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(SCol, OCol, Ignore);
                }
            }
        }

        /// <summary>
        /// Times how long it takes to perform an action in a specified amount of iterations.
        /// </summary>
        /// <returns>Length in milliseconds it took.</returns>
        public static float Benchmark(Action Act, int Iterations)
        {
            Stopwatch Timer = new Stopwatch();
            Timer.Start();
            for (int i = 0; i < Iterations; i++)
            {
                Act.Invoke();
            }
            Timer.Stop();
            return Timer.ElapsedMilliseconds;
        }

        /// <summary>
        /// Easily allows for the modification of a color's alpha value.
        /// </summary>
        public static Color SetAlpha(this Color Source, float Alpha)
        {
            return new Color(Source.r, Source.g, Source.b, Alpha);
        }

        /// <summary>
        /// Determines the angle between 2 Vector3s.
        /// </summary>
        public static float AngleBetweenTwoPoints(this Vector3 VecA, Vector3 VecB)
        {
            return Mathf.Acos(Vector3.Dot(VecA, VecB) / (VecA.magnitude * VecB.magnitude));
        }

        /// <summary>
        /// Determines the angle between 3 Vector3s.
        /// </summary>
        public static float AngleBetweenThreePoints(this Vector3 VecA, Vector3 VecB, Vector3 VecC)
        {
            return AngleBetweenTwoPoints(VecA - VecC, VecB - VecC);
        }

        /// <summary>
        /// Retrieves the surface type of given game object.
        /// </summary>
        public static Surface.SurfaceTypes GetSurfaceType(this GameObject Source)
        {
            Surface SurfaceComp = Source.GetComponent<Surface>();

            if (SurfaceComp)
                return SurfaceComp.type;
            else
                return Surface.SurfaceTypes.Default;
        }
    }}