﻿// File: DevConsole.cs
// Purpose: Provides a public interface for accessing the developer console
// Created by: DavidFDev

#if INPUT_SYSTEM_INSTALLED && ENABLE_INPUT_SYSTEM
#define USE_NEW_INPUT_SYSTEM
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Airship.DevConsole;
using UnityEngine;

using InputKey =
#if USE_NEW_INPUT_SYSTEM
    UnityEngine.InputSystem.Key;
#else
    UnityEngine.KeyCode;
#endif

namespace Airship.DevConsole
{
    /// <summary>
    ///     Interface for accessing the developer console.
    /// </summary>
    [LuauAPI]
    public static class DevConsole
    {
        #region Static fields and constants

        public static DevConsoleMono console;

        public static bool clearConsoleOnServerConnect = true;

        #endregion

        #region Static properties

        /// <summary>
        ///     Whether the dev console is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get => console.ConsoleIsEnabled;
            set
            {
                if (value)
                {
                    EnableConsole();
                    return;
                }

                DisableConsole();
            }
        }

        /// <summary>
        ///     Whether the dev console window is open.
        /// </summary>
        public static bool IsOpen
        {
            get => console.ConsoleIsShowing;
            set
            {
                if (value)
                {
                    console.OpenConsole();
                    return;
                }

                console.CloseConsole();
            }
        }

        /// <summary>
        ///     Whether the dev console window is open and the input field is focused.
        /// </summary>
        public static bool IsOpenAndFocused => console.ConsoleIsShowingAndFocused;

        /// <summary>
        ///     Whether the dev console user-defined key bindings are enabled.
        /// </summary>
        public static bool IsKeyBindingsEnabled
        {
            get => console.BindingsIsEnabled;
            set => console.BindingsIsEnabled = value;
        }

        /// <summary>
        ///     Key used to toggle the dev console window, NULL if no key.
        /// </summary>
        public static InputKey? ToggleKey
        {
            get => console.ConsoleToggleKey;
            set => console.ConsoleToggleKey = value;
        }

        /// <summary>
        ///     Current average FPS. -1 if the fps is not being calculated.
        /// </summary>
        public static int AverageFps => console.AverageFps;

        /// <summary>
        ///     Current average milliseconds per frame (in seconds). -1 if the fps is not being calculated.
        /// </summary>
        public static float AverageMs => console.AverageMs;

        #endregion

        #region Events

        public static event Action OnConsoleEnabled;

        public static event Action OnConsoleDisabled;

        public static event Action<bool> OnConsoleOpened;

        public static event Action<bool> OnConsoleClosed;

        public static event Action OnConsoleFocused;

        public static event Action OnConsoleFocusLost;

        #endregion

        #region Static methods

        /// <summary>
        ///     Add a command to the dev console database.
        /// </summary>
        /// <param name="command">Use Command.Create() to define a command.</param>
        /// <param name="onlyInDevBuild">Whether to only add the command if the project is a development build.</param>
        /// <returns></returns>
        public static bool AddCommand(Command command, bool onlyInDevBuild = false)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return console.AddCommand(command, onlyInDevBuild, true);
        }

        /// <summary>
        ///     Remove a command from the dev console database.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool RemoveCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return console.RemoveCommand(name);
        }

        /// <summary>
        ///     Run a command using the provided input.
        /// </summary>
        /// <param name="input">Input as if it were typed directly into the dev console.</param>
        /// <returns></returns>
        public static bool RunCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            return console.RunCommand(input);
        }

        /// <summary>
        ///     Get a command instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Command GetCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return console.GetCommand(name);
        }

        /// <summary>
        ///     Get a command instance by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool GetCommand(string name, out Command command)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return console.GetCommand(name, out command);
        }

        /// <summary>
        ///     Add a parameter type to the dev console database.
        ///     This will allow the provided type to be used as a parameter in commands.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parseFunc"></param>
        /// <returns></returns>
        public static bool AddParameterType<T>(Func<string, T> parseFunc)
        {
            if (parseFunc == null)
            {
                throw new ArgumentNullException(nameof(parseFunc));
            }

            return console.AddParameterType(typeof(T), s => parseFunc(s));
        }

        /// <summary>
        ///     Log a message to the dev console.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, LogContext context = LogContext.Client, bool prepend = false)
        {
            console.Log(message, context, prepend);
        }

        /// <summary>
        ///     Log a message to the dev console.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="colour"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, Color colour, LogContext context, bool prepend = false)
        {
            console.Log(message, context, prepend, ColorUtility.ToHtmlStringRGBA(colour));
        }

        /// <summary>
        ///     Log a variable to the dev console.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        /// <param name="suffix"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogVariable(string variableName, object value, string suffix = "")
        {
            console.LogVariable(variableName, value, suffix);
        }

        /// <summary>
        ///     Log an exception to the dev console.
        /// </summary>
        /// <param name="exception"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(Exception exception, LogContext context = LogContext.Client, bool prepend = false)
        {
            console.LogException(exception, context, prepend);
        }

        /// <summary>
        ///     Log an error message to the dev console.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(object message, LogContext context = LogContext.Client, bool prepend = false)
        {
            console.LogError(message, context, prepend);
        }

        /// <summary>
        ///     Log a warning message to the dev console.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(object message, LogContext context = LogContext.Client, bool prepend = false)
        {
            console.LogWarning(message, context, prepend);
        }

        /// <summary>
        ///     Log a success message to the dev console.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogSuccess(object message, LogContext context = LogContext.Client)
        {
            console.LogSuccess(message);
        }

        /// <summary>
        ///     Log a message with a seperator bar. A NULL message will log an empty seperator.
        /// </summary>
        /// <param name="message"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogSeperator(object message = null)
        {
            console.LogSeperator(message);
        }

        /// <summary>
        ///     Log a collection in list format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="toString"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogCollection<T>(in IReadOnlyCollection<T> collection, Func<T, string> toString = null, string prefix = "", string suffix = "")
        {
            console.LogCollection(collection, toString, prefix, suffix);
        }

        /// <summary>
        ///     Log the most recently executed command syntax to the dev console.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogCommand()
        {
            console.LogCommand();
        }

        /// <summary>
        ///     Log command syntax to the dev console.
        /// </summary>
        /// <param name="name">Command name.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogCommand(string name)
        {
            console.LogCommand(name);
        }

        /// <summary>
        ///     Set the key used to toggle the dev console window, NULL if no key.
        /// </summary>
        /// <param name="toggleKey"></param>
        public static void SetToggleKey(InputKey? toggleKey)
        {
            console.ConsoleToggleKey = toggleKey;
        }

        /// <summary>
        ///     Disable the key used to toggle the dev console.
        /// </summary>
        public static void DisableToggleKey()
        {
            console.ConsoleToggleKey = null;
        }

        /// <summary>
        ///     Enable the dev console.
        /// </summary>
        public static void EnableConsole()
        {
            console.EnableConsole();
        }

        /// <summary>
        ///     Disable the dev console, making it inaccessible.
        /// </summary>
        public static void DisableConsole()
        {
            console.DisableConsole();
        }

        /// <summary>
        ///     Open the dev console window.
        /// </summary>
        public static void OpenConsole()
        {
            console.OpenConsole();
        }

        /// <summary>
        ///     Close the dev console window.
        /// </summary>
        public static void CloseConsole()
        {
            console.CloseConsole();
        }

        /// <summary>
        ///     Clear the contents of the dev console.
        /// </summary>
        public static void ClearConsole()
        {
            console.ClearConsole();
        }

        public static void ClearActiveConsoleContext() {
            console.ClearActiveConsoleContext();
        }


        /// <summary>
        ///     Set a tracked developer console stat that can be displayed on-screen.
        /// </summary>
        /// <param name="name">Identifier.</param>
        /// <param name="func">Lambda function returning the stat's result that is displayed.</param>
        /// <param name="startEnabled">Whether the stat should start enabled.</param>
        public static void SetTrackedStat(string name, Func<object> func, bool startEnabled = true)
        {
            console.SetTrackedStat(name, func, startEnabled);
        }

        /// <summary>
        ///     Remove a tracked developer console stat.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool RemoveTrackedStat(string name)
        {
            return console.RemoveTrackedStat(name);
        }

        /// <summary>
        ///     Invoke an enumerator as a Unity coroutine. Useful for commands that may not have a reference to a MonoBehaviour.
        /// </summary>
        /// <param name="enumerator"></param>
        public static Coroutine InvokeCoroutine(IEnumerator enumerator)
        {
            return console.StartCoroutine(enumerator);
        }

        /// <summary>
        ///     Invoke an action after a specified time has passed. Useful for commands that may not have a reference to a MonoBehaviour.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public static Coroutine InvokeDelayed(Action action, float delay)
        {
            IEnumerator Invoke()
            {
                yield return new WaitForSeconds(delay);
                action?.Invoke();
            }

            return console.StartCoroutine(Invoke());
        }

        /// <summary>
        ///     Get the user-friendly name of the parameter type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string GetFriendlyName(Type type)
        {
            if (type.IsGenericType)
            {
                Type nullable = Nullable.GetUnderlyingType(type);
                if (nullable != null)
                {
                    return $"{GetFriendlyName(nullable)}?";
                }

                return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyName(x)))}>";
            }

            return type.Name;
        }

        #region Invoke events

        internal static void InvokeOnConsoleEnabled()
        {
            OnConsoleEnabled?.Invoke();
        }

        internal static void InvokeOnConsoleDisabled()
        {
            OnConsoleDisabled?.Invoke();
        }

        internal static void InvokeOnConsoleOpened()
        {
            OnConsoleOpened?.Invoke(true);
        }

        internal static void InvokeOnConsoleClosed()
        {
            OnConsoleClosed?.Invoke(false);
        }

        internal static void InvokeOnConsoleFocused()
        {
            OnConsoleFocused?.Invoke();
        }

        internal static void InvokeOnConsoleFocusLost()
        {
            OnConsoleFocusLost?.Invoke();
        }

        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#pragma warning disable IDE0051
        private static void Init()
#pragma warning restore IDE0051
        {
            console = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/FAB_DevConsole.Instance")).GetComponent<DevConsoleMono>();
        }

        #endregion
    }
}