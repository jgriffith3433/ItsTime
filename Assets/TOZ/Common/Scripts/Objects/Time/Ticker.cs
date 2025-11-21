using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public sealed class Ticker {
		//Variables
		private float interval;
		private float timer;
		public int Counter;

		//Delegates
		public delegate void TickerDelegate();
		public event TickerDelegate OnTicked;

		//Constructors
		public Ticker() : this(1f) { }
		public Ticker(float v) {
			Initialize(v);
		}

		//Methods
		public void Initialize(float v) {
			interval = v;
			timer = 0f;
			Counter = 0;
		}

		public bool Tick(float time) {
			timer += time;
			if(timer >= interval) {
				timer -= timer;
				Counter++;
				if(OnTicked != null)
					OnTicked();
				return true;
			}
			return false;
		}
	}

}