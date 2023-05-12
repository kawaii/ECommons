﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.Logging;
using Dalamud.Utility;
using ECommons.ChatMethods;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;
using ECommons.ExcelServices.TerritoryEnumeration;
using Newtonsoft.Json;
using FFXIVClientStructs.FFXIV.Client.System.String;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Dalamud.Memory;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.MathHelpers;
using PInvoke;
using System.Windows.Forms;
using System.Threading;

namespace ECommons;

public static unsafe class GenericHelpers
{
    public static bool IsKeyPressed(Keys key)
    {
        if (key == Keys.None) return false;
        return Bitmask.IsBitSet(User32.GetKeyState((int)key), 15);
    }

    public static bool IsTarget(this GameObject obj)
    {
        return Svc.Targets.Target != null && Svc.Targets.Target.Address == obj.Address;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOr<T>(this T source, Predicate<T> testFunction)
    {
        return source == null || testFunction(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V GetOrDefault<K, V>(this IDictionary<K, V> dic, K key)
    {
        if(dic.TryGetValue(key, out V value)) return value;
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IncrementOrSet<K>(this IDictionary<K, int> dic, K key, int increment = 1)
    {
        if(dic.ContainsKey(key))
        {
            dic[key] += increment;
        }
        else
        {
            dic[key] = increment;
        }
        return dic[key];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RemoveOtherChars(this string s, string charsToKeep)
    {
        return new string(s.ToArray().Where(charsToKeep.Contains).ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReplaceByChar(this string s, string replaceWhat, string replaceWith, bool replaceWithWhole = false)
    {
        if(replaceWhat.Length != replaceWith.Length && !replaceWithWhole)
        {
            PluginLog.Error($"{nameof(replaceWhat)} and {nameof(replaceWith)} must be same length");
            return s;
        }
        for (int i = 0; i < replaceWhat.Length; i++)
        {
            if (replaceWithWhole)
            {
                s = s.Replace(replaceWhat[i].ToString(), replaceWith);
            }
            else
            {
                s = s.Replace(replaceWhat[i], replaceWith[i]);
            }
        }
        return s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T JSONClone<T>(this T obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }

    [Obsolete($"Use {nameof(Automation.Callback.Fire)} instead", true)]
    public static void Callback(AtkUnitBase* Base, params object[] args)
    {
        var stk = stackalloc AtkValue[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is int v)
            {
                stk[i] = new() { Type = ValueType.Int, Int = v };
            }
            else if (args[i] is uint u)
            {
                stk[i] = new() { Type = ValueType.UInt, UInt = u };
            }
            else if (args[i] is bool b)
            {
                stk[i] = new() { Type = ValueType.Bool, Byte = (byte)(b ? 1 : 0) };
            }
            else if (args[i] is AtkValue av)
            {
                stk[i] = av;
            }
            else
            {
                throw new Exception("Unsupported arguments");
            }
        }
        Base->FireCallback(args.Length, stk);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? ParseInt(this string number)
    {
        if(int.TryParse(number, out var result))
        {
            return result;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Print<T>(this IEnumerable<T> x)
    {
        return x.Select(x => x.ToString()).Join(", ");
    }

    public static void DeleteFileToRecycleBin(string path)
    {
        try
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }
        catch(Exception e)
        {
            e.LogWarning();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V GetSafe<K, V>(this IDictionary<K, V> dic, K key, V Default = default)
    {
        if(dic?.TryGetValue(key, out var value) == true)
        {
            return value;
        }
        return Default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V GetOrCreate<K, V>(this IDictionary<K, V> dictionary, K key) where V:new()
    {
        if (dictionary.TryGetValue(key, out var result))
        {
            return result;
        }
        var newValue = new V();
        dictionary.Add(key, newValue);
        return newValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Each<T>(this IEnumerable<T> collection, Action<T> function)
    {
        foreach(var x in collection)
        {
            function(x);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool If<T>(this T obj, Func<T, bool> func)
    {
        return func(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotNull<T>(this T obj, [NotNullWhen(true)]out T outobj)
    {
        outobj = obj;
        return obj != null;
    }

    public static bool IsOccupied()
    {
        return Svc.Condition[ConditionFlag.Occupied]
               || Svc.Condition[ConditionFlag.Occupied30]
               || Svc.Condition[ConditionFlag.Occupied33]
               || Svc.Condition[ConditionFlag.Occupied38]
               || Svc.Condition[ConditionFlag.Occupied39]
               || Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
               || Svc.Condition[ConditionFlag.OccupiedInEvent]
               || Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
               || Svc.Condition[ConditionFlag.OccupiedSummoningBell]
               || Svc.Condition[ConditionFlag.WatchingCutscene]
               || Svc.Condition[ConditionFlag.WatchingCutscene78]
               || Svc.Condition[ConditionFlag.BetweenAreas]
               || Svc.Condition[ConditionFlag.BetweenAreas51]
               || Svc.Condition[ConditionFlag.InThatPosition]
               || Svc.Condition[ConditionFlag.TradeOpen]
               || Svc.Condition[ConditionFlag.Crafting]
               || Svc.Condition[ConditionFlag.InThatPosition]
               || Svc.Condition[ConditionFlag.Unconscious]
               || Svc.Condition[ConditionFlag.MeldingMateria]
               || Svc.Condition[ConditionFlag.Gathering]
               || Svc.Condition[ConditionFlag.OperatingSiegeMachine]
               || Svc.Condition[ConditionFlag.CarryingItem]
               || Svc.Condition[ConditionFlag.CarryingObject]
               || Svc.Condition[ConditionFlag.BeingMoved]
               || Svc.Condition[ConditionFlag.Emoting]
               || Svc.Condition[ConditionFlag.Mounted2]
               || Svc.Condition[ConditionFlag.Mounting]
               || Svc.Condition[ConditionFlag.Mounting71]
               || Svc.Condition[ConditionFlag.ParticipatingInCustomMatch]
               || Svc.Condition[ConditionFlag.PlayingLordOfVerminion]
               || Svc.Condition[ConditionFlag.ChocoboRacing]
               || Svc.Condition[ConditionFlag.PlayingMiniGame]
               || Svc.Condition[ConditionFlag.Performing]
               || Svc.Condition[ConditionFlag.PreparingToCraft]
               || Svc.Condition[ConditionFlag.Fishing]
               || Svc.Condition[ConditionFlag.Transformed]
               || Svc.Condition[ConditionFlag.UsingHousingFunctions];
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static bool TryDecodeSender(SeString sender, out Sender senderStruct)
    {
        if (sender == null)
        {
            senderStruct = default;
            return false;
        }
        foreach (var x in sender.Payloads)
        {
            if (x is PlayerPayload p)
            {
                senderStruct = new(p.PlayerName, p.World.RowId);
                return true;
            }
        }
        senderStruct = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddonReady(AtkUnitBase* addon)
    {
        return addon->IsVisible && addon->UldManager.LoadedState == AtkLoadState.Loaded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAddonReady(AtkComponentNode* addon)
    {
        return addon->AtkResNode.IsVisible && addon->Component->UldManager.LoadedState == AtkLoadState.Loaded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ExtractText(this Lumina.Text.SeString s, bool onlyFirst = false)
    {
        return s.ToDalamudString().ExtractText(onlyFirst);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ExtractText(this Utf8String s, bool onlyFirst = false)
    {
        var str = MemoryHelper.ReadSeString(&s);
        return str.ExtractText(false);
    }

    public static string ExtractText(this SeString seStr, bool onlyFirst = false)
    {
        StringBuilder sb = new();
        foreach(var x in seStr.Payloads)
        {
            if(x is TextPayload tp)
            {
                sb.Append(tp.Text);
                if (onlyFirst) break;
            }
        }
        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string source, params string[] values)
    {
        return source.StartsWithAny(values, StringComparison.Ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithAny(this string source, StringComparison stringComparison = StringComparison.Ordinal, params string[] values)
    {
        return source.StartsWithAny(values, stringComparison);
    }

    public static bool StartsWithAny(this string source, IEnumerable<string> compareTo, StringComparison stringComparison = StringComparison.Ordinal)
    {
        foreach(var x in compareTo)
        {
            if (source.StartsWith(x, stringComparison)) return true;
        }
        return false;
    }

    public static SeStringBuilder Add(this SeStringBuilder b, IEnumerable<Payload> payloads)
    {
        foreach(var x in payloads)
        {
            b = b.Add(x);
        }
        return b;
    }

    public static bool Toggle<T>(this HashSet<T> hashSet, T value)
    {
        if (hashSet.Contains(value))
        {
            hashSet.Remove(value);
            return false;
        }
        else
        {
            hashSet.Add(value);
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> Split(this string str, int chunkSize)
    {
        return Enumerable.Range(0, str.Length / chunkSize)
            .Select(i => str.Substring(i * chunkSize, chunkSize));
    }

    public static string GetTerritoryName(this uint terr)
    {
        var t = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(terr);
        return $"{terr} | {t?.ContentFinderCondition?.Value?.Name?.ToString().Default(t?.PlaceName.Value?.Name?.ToString())}";
    }

    public static T FirstOr0<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        foreach(var x in collection)
        {
            if (predicate(x))
            {
                return x;
            }
        }
        return collection.First();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Default(this string s, string defaultValue)
    {
        if (string.IsNullOrEmpty(s)) return defaultValue;
        return s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string s, string other)
    {
        return s.Equals(other, StringComparison.OrdinalIgnoreCase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NullWhenEmpty(this string s)
    {
        return s == string.Empty ? null : s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    public static IEnumerable<R> SelectMulti<T, R>(this IEnumerable<T> values, params Func<T, R>[] funcs)
    {
        foreach(var v in values)
        foreach(var x in funcs)
        {
                yield return x(v);
        }
    }

    public static bool TryGetWorldByName(string world, out Lumina.Excel.GeneratedSheets.World worldId) 
    {
        if(Svc.Data.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>().TryGetFirst(x => x.Name.ToString().Equals(world, StringComparison.OrdinalIgnoreCase), out var w))
        {
            worldId = w;
            return true;
        }
        worldId = default;
        return false;
    }

    public static Vector4 Invert(this Vector4 v)
    {
        return v with { X = 1f - v.X, Y = 1f - v.Y, Z = 1f - v.Z };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUint(this Vector4 color)
    {
        return ImGui.ColorConvertFloat4ToU32(color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this uint color)
    {
        return ImGui.ColorConvertU32ToFloat4(color);
    }

    public static ref int ValidateRange(this ref int i, int min, int max)
    {
        if (i > max) i = max;
        if (i < min) i = min;
        return ref i;
    }

    public static ref float ValidateRange(this ref float i, float min, float max)
    {
        if (i > max) i = max;
        if (i < min) i = min;
        return ref i;
    }

    public static void LogWarning(this Exception e)
    {
        PluginLog.Warning($"{e.Message}\n{e.StackTrace ?? ""}");
    }

    public static void Log(this Exception e)
    {
        PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
    }

    public static void Log(this Exception e, string ErrorMessage)
    {
        PluginLog.Error($"{ErrorMessage}\n{e.Message}\n{e.StackTrace ?? ""}");
    }

    public static void LogDuo(this Exception e)
    {
        DuoLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
    }

    public static bool IsNoConditions()
    {
        if (!Svc.Condition[ConditionFlag.NormalConditions]) return false;
        for(var i = 2; i < 100; i++)
        {
            if (i == (int)ConditionFlag.ParticipatingInCrossWorldPartyOrAlliance) continue;
            if (Svc.Condition[i]) return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Invert(this bool b, bool invert)
    {
        return invert ? !b : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
    {
        foreach(var x in values)
        {
            if (!source.Contains(x)) return false;
        }
        return true;
    }

    public static void ShellStart(string s)
    {
        Safe(delegate
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = s,
                UseShellExecute = true
            });
        }, (e) =>
        {
            Notify.Error($"Could not open {s.Cut(60)}\n{e}");
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Cut(this string s, int num)
    {
        if (s.Length <= num) return s;
        return s[0..num] + "...";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetParsedSeSetingColor(int percent)
    {
        if(percent < 25)
        {
            return 3;
        }
        else if(percent < 50)
        {
            return 45;
        }
        else if(percent < 75)
        {
            return 37;
        }
        else if(percent < 95)
        {
            return 541;
        }
        else if(percent < 99)
        {
            return 500;
        }
        else if (percent == 99)
        {
            return 561;
        }
        else if (percent == 100)
        {
            return 573;
        }
        else
        {
            return 518;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Repeat(this string s, int num)
    {
        StringBuilder str = new();
        for(var i = 0; i < num; i++)
        {
            str.Append(s);
        }
        return str.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this IEnumerable<string> e, string separator)
    {
        return string.Join(separator, e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(System.Action a, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(System.Action a, Action<string, object[]> logAction)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            logAction($"{e.Message}\n{e.StackTrace ?? ""}", Array.Empty<object>());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(System.Action a, Action<string> fail, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            try
            {
                fail(e.Message);
            }
            catch(Exception ex)
            {
                PluginLog.Error("Error while trying to process error handler:");
                PluginLog.Error($"{ex.Message}\n{ex.StackTrace ?? ""}");
                suppressErrors = false;
            }
            if (!suppressErrors) PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    public static bool TryExecute(System.Action a)
    {
        try
        {
            a();
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            return false;
        }
    }

    public static bool TryExecute<T>(Func<T> a, out T result)
    {
        try
        {
            result = a();
            return true;
        }
        catch (Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            result = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this IEnumerable<T> obj, params T[] values)
    {
        foreach (var x in values)
        {
            if (obj.Contains(x))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny<T>(this IEnumerable<T> obj, IEnumerable<T> values)
    {
        foreach (var x in values)
        {
            if (obj.Contains(x))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, IEnumerable<string> values)
    {
        foreach (var x in values)
        {
            if (obj.Contains(x))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, params string[] values)
    {
        foreach (var x in values)
        {
            if (obj.Contains(x))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string obj, StringComparison comp, params string[] values)
    {
        foreach (var x in values)
        {
            if (obj.Contains(x, comp))
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, params T[] values)
    {
        return values.Any(x => x.Equals(obj));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, params string[] values)
    {
        return values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAny(this string obj, IEnumerable<string> values)
    {
        return values.Any(x => x.Equals(obj, StringComparison.OrdinalIgnoreCase));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsAny<T>(this T obj, IEnumerable<T> values)
    {
        return values.Any(x => x.Equals(obj));
    }

    public static IEnumerable<K> FindKeysByValue<K, V>(this IDictionary<K, V> dictionary, V value)
    {
        foreach(var x in dictionary)
        {
            if (value.Equals(x.Value))
            {
                yield return x.Key;
            }
        }
    }

    public static bool TryGetFirst<K, V>(this IDictionary<K, V> dictionary, Func<KeyValuePair<K, V>, bool> predicate, out KeyValuePair<K, V> keyValuePair)
    {
        try
        {
            keyValuePair = dictionary.First(predicate);
            return true;
        }
        catch(Exception)
        {
            keyValuePair = default;
            return false;
        }
    }

    public static bool TryGetFirst<K>(this IEnumerable<K> enumerable, out K value)
    {
        try
        {
            value = enumerable.First();
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public static bool TryGetFirst<K>(this IEnumerable<K> enumerable, Func<K, bool> predicate, out K value)
    {
        try
        {
            value = enumerable.First(predicate);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public static bool TryGetLast<K>(this IEnumerable<K> enumerable, Func<K, bool> predicate, out K value)
    {
        try
        {
            value = enumerable.Last(predicate);
            return true;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public static bool TryGetAddonByName<T>(string Addon, out T* AddonPtr) where T : unmanaged
    {
        var a = Svc.GameGui.GetAddonByName(Addon, 1);
        if (a == IntPtr.Zero)
        {
            AddonPtr = null;
            return false;
        }
        else
        {
            AddonPtr = (T*)a;
            return true;
        }
    }

    public static bool IsSelectItemEnabled(AtkTextNode* textNodePtr)
    {
        var col = textNodePtr->TextColor;
        //EEE1C5FF
        return (col.A == 0xFF && col.R == 0xEE && col.G == 0xE1 && col.B == 0xC5)
            //7D523BFF
            || (col.A == 0xFF && col.R == 0x7D && col.G == 0x52 && col.B == 0x3B)
            || (col.A == 0xFF && col.R == 0xFF && col.G == 0xFF && col.B == 0xFF)
            // EEE1C5FF
            || (col.A == 0xFF && col.R == 0xEE && col.G == 0xE1 && col.B == 0xC5);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete($"Use MemoryHelper.ReadSeString")]
    public static unsafe SeString ReadSeString(Utf8String* utf8String) => MemoryHelper.ReadSeString(utf8String);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete($"Use MemoryHelper.ReadSeString")]
    public static SeString ReadSeString(IntPtr memoryAddress, int maxLength) => MemoryHelper.ReadSeString(memoryAddress, maxLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete($"Use MemoryHelper.ReadRaw")]
    public static void ReadRaw(IntPtr memoryAddress, int length, out byte[] value) => value = MemoryHelper.ReadRaw(memoryAddress, length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete($"Use MemoryHelper.ReadRaw")]
    public static byte[] ReadRaw(IntPtr memoryAddress, int length) => MemoryHelper.ReadRaw(memoryAddress, length);
}
