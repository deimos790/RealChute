﻿using System;
using System.Collections.Generic;
using RealChute.Extensions;
using UnityEngine;

namespace RealChute
{
    public abstract class EnumConstraint<TEnum> where TEnum : class
    {
        /// <summary>
        /// Generic enum conversion utility class
        /// </summary>
        private class EnumConverter
        {
            #region Fields
            /// <summary>
            /// Stores the string -> enum conversion
            /// </summary>
            private Dictionary<string, TEnum> values = new Dictionary<string, TEnum>();

            /// <summary>
            /// Stores the enum -> string conversion
            /// </summary>
            private Dictionary<TEnum, string> names = new Dictionary<TEnum, string>();

            /// <summary>
            /// Stores the index of each member with it's string representation
            /// </summary>
            private Dictionary<string, int> nameIndexes = new Dictionary<string, int>();

            /// <summary>
            /// Stores the index of each member with the member as the key
            /// </summary>
            private Dictionary<TEnum, int> indexes = new Dictionary<TEnum, int>();

            /// <summary>
            /// Stores the index of each member as the key with the string representation of it as the value
            /// </summary>
            private Dictionary<int, string> atNames = new Dictionary<int, string>();

            /// <summary>
            /// Stores the index of each member as the key with the member as the value
            /// </summary>
            private Dictionary<int, TEnum> atValues = new Dictionary<int, TEnum>();

            /// <summary>
            /// The name of the enum values correctly ordered for index search
            /// </summary>
            public string[] orderedNames = new string[0];

            /// <summary>
            /// The values of the Enum correctly ordered for index search
            /// </summary>
            public TEnum[] orderedValues = new TEnum[0];
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new EnumConvertor from the given type
            /// </summary>
            /// <param name="enumType">Type of converter. Must be an enum type.</param>
            public EnumConverter(Type enumType)
            {
                if (enumType == null) { throw new ArgumentNullException("enumType", "Enum conversion type cannot be null"); }
                Array values = Enum.GetValues(enumType);
                this.orderedNames = new string[values.Length];
                this.orderedValues = new TEnum[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    TEnum value = (TEnum)values.GetValue(i);
                    string name = Enum.GetName(enumType, value);
                    this.orderedNames[i] = name;
                    this.orderedValues[i] = value;
                    this.values.Add(name, value);
                    this.names.Add(value, name);
                    this.indexes.Add(value, i);
                    this.nameIndexes.Add(name, i);
                    this.atValues.Add(i, value);
                    this.atNames.Add(i, name);
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Tries to parse the given Enum member and stores the result in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="name">String to parse</param>
            /// <param name="value">Value to store the result into</param>
            public bool TryGetValue<T>(string name, out T value) where T : struct, TEnum
            {
                TEnum result;
                bool success = values.TryGetValue(name, out result);
                value = (T)result;
                return success;

            }

            /// <summary>
            /// Tries to get the string name of the Enum value and stores it in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="value">Enum to get the string for</param>
            /// <param name="name">Value to store the result into</param>
            public bool TryGetName<T>(T value, out string name) where T : struct, TEnum
            {
                return this.names.TryGetValue(value, out name);
            }

            /// <summary>
            /// Tries to get the Enum value at the given index
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="index">Index of the value to get</param>
            /// <param name="value">Value to store the result into</param>
            public bool TryGetValueAt<T>(int index, out T value) where T : struct, TEnum
            {
                TEnum result;
                bool success = this.atValues.TryGetValue(index, out result);
                value = (T)result;
                return success;
            }

            /// <summary>
            /// Tries to get the Enum member name at the given index
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="index">Index of the name to find</param>
            /// <param name="name">Value to store the result into</param>
            public bool TryGetNameAt<T>(int index, out string name) where T : struct, TEnum
            {
                return this.atNames.TryGetValue(index, out name);
            }

            /// <summary>
            /// Finds the index of a given enum name
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="name">Enum member name to find the index of</param>
            public int IndexOf<T>(string name) where T : struct, TEnum
            {
                int index;
                if (!this.nameIndexes.TryGetValue(name, out index)) { return -1; }
                return index;
            }

            /// <summary>
            /// Finds the index of a given Enum member
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="value">Enum value to find the index of</param>
            public int IndexOf<T>(T value) where T : struct, TEnum
            {
                int index;
                if (!this.indexes.TryGetValue(value, out index)) { return -1; }
                return index;
            }
            #endregion
        }

        #region Fields
        /// <summary>
        /// Holds all the known enum converters
        /// </summary>
        private static Dictionary<Type, EnumConverter> converters = new Dictionary<Type, EnumConverter>();
        #endregion

        #region Methods
        /// <summary>
        /// Returns the converter of the given type or creates one if there are none
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="enumType">Type of the enum conversion</param>
        private static EnumConverter GetConverter<T>()
        {
            EnumConverter converter;
            Type enumType = typeof(T);
            if (!converters.TryGetValue(enumType, out converter))
            {
                converter = new EnumConverter(enumType);
                converters.Add(enumType, converter);
            }
            return converter;
        }

        /// <summary>
        /// Returns the string value of an Enum
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="value">Enum value to convert to string</param>
        public static string GetName<T>(T value) where T : struct, TEnum
        {
            string result;
            GetConverter<T>().TryGetName(value, out result);
            return result;
        }

        /// <summary>
        /// Parses the given string to the given Enum type 
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="name">String to parse</param>
        public static T GetValue<T>(string name) where T : struct, TEnum
        {
            T result;
            GetConverter<T>().TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// Finds the string name of the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the name to find</param>
        public static string GetNameAt<T>(int index) where T : struct, TEnum
        {
            string name;
            GetConverter<T>().TryGetNameAt<T>(index, out name);
            return name;
        }

        /// <summary>
        /// Gets the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the element to get</param>
        public static T GetValueAt<T>(int index) where T : struct, TEnum
        {
            T result;
            GetConverter<T>().TryGetValueAt(index, out result);
            return result;
        }

        /// <summary>
        /// Returns the string representation of each enum member in order
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        public static string[] GetNames<T>() where T : struct, TEnum
        {
            return GetConverter<T>().orderedNames;
        }

        /// <summary>
        /// Gets an array of all the values of the Enum
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        public static T[] GetValues<T>() where T : struct, TEnum
        {
            return GetConverter<T>().orderedValues.ConvertAll(v => (T)v);
        }

        /// <summary>
        /// Returns the index of the Enum value of the given name
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="name">Name of the element to find</param>
        public static int IndexOf<T>(string name) where T : struct, TEnum
        {
            return GetConverter<T>().IndexOf<T>(name);
        }

        /// <summary>
        /// Returns the index of the Enum member of the given value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="value">Value to find the index of</param>
        public static int IndexOf<T>(T value) where T : struct, TEnum
        {
            return GetConverter<T>().IndexOf(value);
        }

        /// <summary>
        /// Creates a GUILayout SelectionGrid which shows the names of all the members of an Enum an returns the selected value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="selected">Currently selected Enum member</param>
        /// <param name="xCount">Amount of boxes on one line</param>
        /// <param name="style">GUIStyle of th boxes</param>
        /// <param name="options">GUILayout options</param>
        public static T SelectionGrid<T>(T selected, int xCount, GUIStyle style, params GUILayoutOption[] options) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter<T>();
            int index = converter.IndexOf(selected);
            index = GUILayout.SelectionGrid(index, converter.orderedNames, xCount, style, options);
            converter.TryGetValueAt(index, out selected);
            return selected;
        }
        #endregion
    }

    public sealed class EnumUtils : EnumConstraint<Enum>
    {
        /* Nothing to see here, this is just a dummy class to force T to be an Enum.
         * The actual implementation is in EnumConstraint */

        /// <summary>
        /// Prevents object instantiation
        /// </summary>
        private EnumUtils() { }
    }
}