using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public sealed class Chronometer {
		//Variables
		private float start;
		private float end;
		private bool isRunning;

		//Constructors
		public Chronometer() {
			Initialize();
		}

		//Methods
		public void Initialize() {
			start = 0f;
			end = 0f;
			isRunning = false;
		}

		public void Start() {
			if(!isRunning) {
				isRunning = true;
				start = Time.time;
			}
		}

		public void Stop() {
			if(isRunning) {
				isRunning = false;
				end = Time.time;
			}
		}

		public float Elapsed() {
			return (isRunning ? (Time.time - start) : (end - start));
		}
	}

}