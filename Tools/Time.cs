﻿namespace SMPL.Tools
{
	/// <summary>
	/// A class that tracks time and calculates time related values.
	/// </summary>
	public static class Time
	{
		/// <summary>
		/// The type of time convertion going from one time unit to another. This is used by <see cref="ToTime"/>.
		/// </summary>
		public enum Convertion
		{
			MillisecondsToSeconds, MillisecondsToMinutes,
			SecondsToMilliseconds, SecondsToMinutes, SecondsToHours,
			MinutesToMilliseconds, MinutesToSeconds, MinutesToHours, MinutesToDays,
			HoursToSeconds, HoursToMinutes, HoursToDays, HoursToWeeks,
			DaysToMinutes, DaysToHours, DaysToWeeks,
			WeeksToHours, WeeksToDays
		}
		[Flags]
		public enum Unit
		{
			Day = 1,
			Hour = 2,
			Minute = 4,
			Second = 8,
			Millisecond = 16,
			AM_PM = 32,
			DisplayAM_PM = 64,
		}
		/// <summary>
		/// The real time clock taken from <see cref="DateTime.Now"/> in seconds ranged
		/// [0 - 86399]<br></br>or in clock hours ranged [12 AM, 00:00, 24:00 - 11:59:59 AM, 23:59:59].
		/// </summary>
		public static float Clock { get; private set; }
		/// <summary>
		/// Limits the <see cref="Delta"/> (default is 0.1 which is 10 FPS). This prevents huge values generated by long periods of not
		/// calling <see cref="Update"/> (the game lagging for a second or two or whenever the user drags the <see cref="Game.Window"/> for example).
		/// </summary>
		public static float MaxDelta { get; set; } = 0.1f;
		/// <summary>
		/// The time in seconds since the last frame/tick/update. This is useful for multiplying a step value against it in continuous calculations
		/// so that the step value is consistent on all systems.<br></br><br></br>
		/// - Example: An <see cref="ThingInstance"/> moving with the speed of 1 pixel per frame/tick/update in a game running at 60 FPS will be moving with 60
		/// pixels per second.<br></br> But on a game running at 120 FPS - it will be moving with 120 pixels per second or twice as fast.<br></br>
		/// This also  means that some users with low-end hardware will appear to play the game in slow motion
		/// (when the FPS drops bellow 40, 30, 20).<br></br>
		/// The step value of that <see cref="ThingInstance"/> (in this case the speed of '1 pixel per frame/tick/update') should be multiplied with
		/// <see cref="Delta"/> to prevent it from messing with the gameplay.<br></br><br></br>
		/// - Note: The continuous movement methods in <see cref="Extensions"/> are already accounting the delta time
		/// in their calculations with an argument determining whether they are FPS dependent.
		/// </summary>
		public static float Delta { get; private set; }
		/// <summary>
		/// The frames/ticks/updates per second.
		/// </summary>
		public static float FPS { get; private set; }
		/// <summary>
		/// The average FPS since the start of the game. See <see cref="FPS"/> for more info.
		/// </summary>
		public static float AverageFPS { get; private set; }
		/// <summary>
		/// The seconds that have passed since the start of the game.
		/// </summary>
		public static float GameClock { get; private set; }
		/// <summary>
		/// The amount of rendered frames since the start of the game.
		/// </summary>
		public static uint FrameCount { get; private set; }

		public static string ToClock(this float seconds, string separator = ":", Unit units = Unit.Hour | Unit.Minute | Unit.Second)
		{
			var ts = TimeSpan.FromSeconds(seconds);
			var result = "";
			var counter = 0;

			if(units.HasFlag(Unit.Day))
			{
				var val = (int)ts.TotalDays;
				result += $"{val:D2}";
				counter++;
			}
			if(units.HasFlag(Unit.Hour))
			{
				var sep = counter > 0 ? separator : "";
				var val = counter == 0 ? (int)ts.TotalHours : (units.HasFlag(Unit.AM_PM) ? ts.Hours.Wrap(12) : ts.Hours);
				val = val == 0 ? 12 : val;
				result += $"{sep}{val:D2}";
				counter++;
			}
			if(units.HasFlag(Unit.Minute))
			{
				var sep = counter > 0 ? separator : "";
				var val = counter == 0 ? (int)ts.TotalMinutes : ts.Minutes;
				result += $"{sep}{val:D2}";
				counter++;
			}
			if(units.HasFlag(Unit.Second))
			{
				var sep = counter > 0 ? separator : "";
				var val = counter == 0 ? (int)ts.TotalSeconds : ts.Seconds;
				result += $"{sep}{val:D2}";
				counter++;
			}
			if(units.HasFlag(Unit.Millisecond))
			{
				var val = counter == 0 ? (int)ts.TotalMilliseconds : ts.Milliseconds;
				var dot = units.HasFlag(Unit.Second) ? "." : "";
				var sep = dot == "" && counter > 0 ? separator : "";
				result += $"{sep}{dot}{val:D3}";
				counter++;
			}
			if(units.HasFlag(Unit.DisplayAM_PM))
			{
				var sep = counter > 0 ? " " : "";
				var str = ts.Hours >= 12 ? "PM" : "AM";
				result += $"{sep}{str}";
			}
			return result;
		}
		/// <summary>
		/// Converts a <paramref name="time"/> from one time unit to another (chosen by <paramref name="convertType"/>). Then returns the result.
		/// </summary>
		public static float ToTime(this float time, Convertion convertType)
		{
			return convertType switch
			{
				Convertion.MillisecondsToSeconds => time / 1000,
				Convertion.MillisecondsToMinutes => time / 1000 / 60,
				Convertion.SecondsToMilliseconds => time * 1000,
				Convertion.SecondsToMinutes => time / 60,
				Convertion.SecondsToHours => time / 3600,
				Convertion.MinutesToMilliseconds => time * 60000,
				Convertion.MinutesToSeconds => time * 60,
				Convertion.MinutesToHours => time / 60,
				Convertion.MinutesToDays => time / 1440,
				Convertion.HoursToSeconds => time * 3600,
				Convertion.HoursToMinutes => time * 60,
				Convertion.HoursToDays => time / 24,
				Convertion.HoursToWeeks => time / 168,
				Convertion.DaysToMinutes => time * 1440,
				Convertion.DaysToHours => time * 24,
				Convertion.DaysToWeeks => time / 7,
				Convertion.WeeksToHours => time * 168,
				Convertion.WeeksToDays => time * 7,
				_ => 0,
			};
		}

		public static void CallAfter(float seconds, Action method, bool isRepeating = false)
		{
			timers.Add(new Timer(seconds, isRepeating, method));
		}
		public static void CancelCall(Action method)
		{
			var timersToRemove = new List<Timer>();
			for(int i = 0; i < timers.Count; i++)
				if(timers[i].method == method)
					timersToRemove.Add(timers[i]);

			for(int i = 0; i < timersToRemove.Count; i++)
				timers.Remove(timersToRemove[i]);
		}
		public static void OffsetCall(float secondsOffset, Action method)
		{
			for(int i = 0; i < timers.Count; i++)
				if(timers[i].method == method)
					timers[i].delay += secondsOffset;
		}

		#region Backend
		internal class Timer
		{
			public Action method;
			public Clock clock = new();
			public bool isLooping;
			public float delay;
			public bool IsDisposed => method == null;

			public Timer(float seconds, bool isLooping, Action method)
			{
				delay = seconds;
				this.isLooping = isLooping;
				this.method = method;
			}

			public void TryTrigger()
			{
				if(clock.ElapsedTime.AsSeconds() < delay)
					return;

				Trigger(true);

				if(isLooping == false)
					Dispose();
			}
			public void Restart()
			{
				clock.Restart();
			}
			public void Trigger(bool reset)
			{
				method?.Invoke();

				if(reset)
					Restart();
			}

			private void Dispose()
			{
				method = null;
				clock.Dispose();
			}
		}

		private readonly static List<Timer> timers = new();
		private static readonly Clock time = new(), delta = new(), updateFPS = new();

		internal static void Update()
		{
			GameClock = (float)time.ElapsedTime.AsSeconds();
			Delta = ((float)delta.ElapsedTime.AsSeconds()).Limit(0, MathF.Max(0, MaxDelta));
			delta.Restart();
			Clock = (float)DateTime.Now.TimeOfDay.TotalSeconds;
			if((float)updateFPS.ElapsedTime.AsSeconds() > 0.1f)
			{
				updateFPS.Restart();
				FPS = 1f / Delta;
				AverageFPS = FrameCount / GameClock;
			}
			FrameCount++;

			var toBeRemoved = new List<Timer>();
			for(int i = 0; i < timers.Count; i++)
			{
				var timer = timers[i];

				timer.TryTrigger();

				if(timer.IsDisposed)
					toBeRemoved.Add(timer);
			}

			for(int i = 0; i < toBeRemoved.Count; i++)
			{
				var timer = toBeRemoved[i];
				timers.Remove(timer);
			}
		}
		#endregion
	}
}

