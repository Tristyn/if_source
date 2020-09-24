using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Array of 32 bits. Fully unmanaged. Defaults to zeroes. Enumerable in C# 7.
/// </summary>
/// 
/// <author>
/// Jackson Dunstan, https://JacksonDunstan.com/articles/5172
/// </author>
/// 
/// <license>
/// MIT
/// </license>
[Serializable]
public struct BitArray32 : IEquatable<BitArray32>
{
    /// <summary>
    /// Integer whose bits make up the array
    /// </summary>
    [SerializeField]
    uint bits;

    /// <summary>
    /// Create the array with the given bits
    /// </summary>
    /// <param name="bits">
    /// Bits to make up the array
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitArray32(uint bits)
    {
        this.bits = bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator uint(BitArray32 bitArray)
    {
        return bitArray.bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitArray32(uint bits)
    {
        return new BitArray32(bits);
    }

    /// <summary>
    /// Get or set the bit at the given index. For faster getting of multiple
    /// bits, use <see cref="GetBits(uint)"/>. For faster setting of single
    /// bits, use <see cref="SetBit(int)"/> or <see cref="UnsetBit(int)"/>. For
    /// faster setting of multiple bits, use <see cref="SetBits(uint)"/> or
    /// <see cref="UnsetBits(uint)"/>.
    /// </summary>
    /// <param name="index">
    /// Index of the bit to get or set
    /// </param>
    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            RequireIndexInBounds(index);
            uint mask = 1u << index;
            return (bits & mask) == mask;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            RequireIndexInBounds(index);
            uint mask = 1u << index;
            if (value)
            {
                bits |= mask;
            }
            else
            {
                bits &= ~mask;
            }
        }
    }

    /// <summary>
    /// Get the length of the array
    /// </summary>
    /// <value>
    /// The length of the array. Always 32.
    /// </value>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return 32;
        }
    }

    /// <summary>
    /// Set a single bit to 1
    /// </summary>
    /// <param name="index">
    /// Index of the bit to set. Asserts if not on [0:31].
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int index)
    {
        RequireIndexInBounds(index);
        uint mask = 1u << index;
        bits |= mask;
    }

    /// <summary>
    /// Set a single bit to 0
    /// </summary>
    /// <param name="index">
    /// Index of the bit to unset. Asserts if not on [0:31].
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsetBit(int index)
    {
        RequireIndexInBounds(index);
        uint mask = 1u << index;
        bits &= ~mask;
    }

    /// <summary>
    /// Get all the bits that match a mask
    /// </summary>
    /// <param name="mask">
    /// Mask of bits to get
    /// </param>
    /// <returns>
    /// The bits that match the given mask
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetBits(uint mask)
    {
        return bits & mask;
    }

    /// <summary>
    /// Set all the bits that match a mask to 1
    /// </summary>
    /// <param name="mask">
    /// Mask of bits to set
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBits(uint mask)
    {
        bits |= mask;
    }

    /// <summary>
    /// Set all the bits that match a mask to 0
    /// </summary>
    /// <param name="mask">
    /// Mask of bits to unset
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsetBits(uint mask)
    {
        bits &= ~mask;
    }

    /// <summary>
    /// Check if this array equals an object
    /// </summary>
    /// <param name="obj">
    /// Object to check. May be null.
    /// </param>
    /// <returns>
    /// If the given object is a BitArray32 and its bits are the same as this
    /// array's bits
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        return obj is BitArray32 && bits == ((BitArray32)obj).bits;
    }

    /// <summary>
    /// Check if this array equals another array
    /// </summary>
    /// <param name="arr">
    /// Array to check
    /// </param>
    /// <returns>
    /// If the given array's bits are the same as this array's bits
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BitArray32 arr)
    {
        return bits == arr.bits;
    }

    /// <summary>
    /// Get the hash code of this array
    /// </summary>
    /// <returns>
    /// The hash code of this array, which is the same as
    /// the hash code of <see cref="bits"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return bits.GetHashCode();
    }

    /// <summary>
    /// Get a string representation of the array
    /// </summary>
    /// <returns>
    /// A newly-allocated string representing the bits of the array.
    /// </returns>
    public override string ToString()
    {
        const string header = "BitArray32{";
        const int headerLen = 11; // must be header.Length
        char[] chars = new char[headerLen + 32 + 1];
        int i = 0;
        for (; i < headerLen; ++i)
        {
            chars[i] = header[i];
        }
        for (uint num = 1u << 31; num > 0; num >>= 1, ++i)
        {
            chars[i] = (bits & num) != 0 ? '1' : '0';
        }
        chars[i] = '}';
        return new string(chars);
    }

    /// <summary>
    /// Population count a.k.a. hamming weight. Count the number of high bits in an integer
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PopCount()
    {
        return Mathx.PopCount(bits);
    }

    /// <summary>
    /// Assert if the given index isn't in bounds
    /// </summary>
    /// <param name="index">
    /// Index to check
    /// </param>
    [BurstDiscard]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RequireIndexInBounds(int index)
    {
        Assert.IsTrue(
            index >= 0 && index < 32,
            "Index out of bounds");
    }
}