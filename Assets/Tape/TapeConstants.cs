using System;

public static class TapeConstants
{
	private static readonly ushort[] _indexOfSymbols;

	/// <summary>The number of cells in a chunk. Must be the same in the pixel shader.</summary>
	public const int CellsPerChunk = 32;

	/// <summary>Symbols in the tape symbols texture.</summary>
	public const string TapeSymbols = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ~+-*/%=!#&$?";

	/// <summary>Finds the index of a symbol in TapeSymbols array, complexity is O(1).</summary>
	public static int IndexOfSymbol(char character) {
		return (uint)character < (uint)_indexOfSymbols.Length ? _indexOfSymbols [character] : -1;
	}

	static TapeConstants() {
		char maxSymbol = '\0';
		for (int i = 0; i < TapeSymbols.Length; ++i) {
			if (TapeSymbols [i] > maxSymbol)
				maxSymbol = TapeSymbols [i];
		}
		_indexOfSymbols = new ushort[maxSymbol + 1];
		for (int i = 0; i < TapeSymbols.Length; ++i)
			_indexOfSymbols [TapeSymbols [i]] = (ushort)i;
	}
}

