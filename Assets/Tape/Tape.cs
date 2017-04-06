using System;
using System.Collections.Generic;

public class Tape
{
	private Dictionary<int, char> _memory;

	public delegate void TapeChangeEventHandler (int? address);

	public event TapeChangeEventHandler TapeChanged;

	public Tape ()
	{
		_memory = new Dictionary<int, char> ();
	}

	public bool Clear() {
		if (_memory.Count == 0)
			return false;
		_memory.Clear ();
		OnTapeChanged (null);
		return true;
	}

	public char GetCell (int address)
	{
		char c;
		_memory.TryGetValue (address, out c);
		return c != 0 ? c : ' ';
	}

	public bool SetCell (int address, char value)
	{
		if (TapeConstants.IndexOfSymbol (value) < 0)
			value = ' ';

		if (value == ' ') {
			if (!_memory.Remove (value))
				return false;
		} else {
			char c;
			_memory.TryGetValue (address, out c);
			if (c == value)
				return false;
		}
		OnTapeChanged (address);
		return true;
	}

	public bool SetCells (int index, string value)
	{
		bool result = false;
		for (int i = 0; i < value.Length; ++i)
			if (SetCell (i, value [i]))
				result = true;
		return result;
	}

	protected virtual void OnTapeChanged (int? address)
	{
		var handler = TapeChanged;
		if (handler != null)
			handler (address);
	}
}

