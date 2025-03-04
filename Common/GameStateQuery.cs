#nullable enable

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using StardewValley;
using StardewValley.Locations;

using StardewModdingAPI;

using SObject = StardewValley.Object;

namespace Leclair.Stardew.Common;

/// <summary>
/// A reimplementation of Stardew Valley 1.6's GameStateQuery system. This
/// implementation is based on the <see href="https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6#Game_state_queries">1.6 migration documentation</see>
/// available on the modding wiki, and has been lightly tested against known
/// GameStateQueries used in 1.6 to ensure they evaluate and return sane
/// results. It's probably got some bugs and/or differences in behavior from
/// the behavior of the official GameStateQuery class in 1.6.
/// </summary>
public static class GameStateQuery {

	#region Storage

	public static readonly Dictionary<string, Func<string[], Func<GameState, bool>>> Conditions = new();

	#endregion

	#region Registration

	public static void RegisterCondition(string key, Func<int, GameState, bool> method) {
		RegisterCondition(key, args => {
			int value = int.Parse(args[0]);
			return state => method(value, state);
		});
	}

	public static void RegisterCondition(string key, Func<int, int, GameState, bool> method) {
		RegisterCondition(key, args => {
			int item1 = int.Parse(args[0]);
			int item2 = int.Parse(args[1]);
			return state => method(item1, item2, state);
		});
	}

	public static void RegisterCondition(string key, Func<string, GameState, bool> method) {
		RegisterCondition(key, args => {
			string joined = string.Join(' ', args);
			return state => method(joined, state);
		});
	}

	public static void RegisterCondition(string key, Func<Farmer, string, GameState, bool> method) {
		RegisterCondition(key, args => {
			string joined = string.Join(' ', args[1..]);
			return state => CheckMatchingPlayers(args[0], state, farmer => method(farmer, joined, state));
		});
	}

	public static void RegisterCondition(string key, Func<Farmer, int, GameState, bool> method) {
		RegisterCondition(key, args => {
			int value = int.Parse(args[1]);
			return state => CheckMatchingPlayers(args[0], state, farmer => method(farmer, value, state));
		});
	}

	public static void RegisterCondition(string key, Func<string[], Func<GameState, bool>> method) {
		lock ((Conditions as ICollection).SyncRoot) {
			if (!Conditions.ContainsKey(key))
				Conditions.Add(key, method);
		}
	}

	#endregion

	#region Initialization

	private static bool Initialized = false;

	private static void Initialize() {
		if (Initialized)
			return;

		Initialized = true;

		RegisterCondition("TRUE", (_) => (_) => true);
		RegisterCondition("FALSE", (_) => (_) => false);

		RegisterCondition("DAY_OF_MONTH", (value, state) =>
			state.date.DayOfMonth == value
		);

		RegisterCondition("DAY_OF_WEEK", (value, state) =>
			(state.date.DayOfMonth % 7) == value
		);

		RegisterCondition("DAYS_PLAYED", (value, state) =>
			state.date.TotalDays + 1 >= value
		);

		RegisterCondition("IS_FESTIVAL_DAY", (offset, state) => {
			WorldDate date;
			if (offset == 0)
				date = state.date;
			else {
				date = new WorldDate(state.date);
				date.TotalDays += offset;
			}

			return Utility.isFestivalDay(date.DayOfMonth, date.Season);
		});

		RegisterCondition("SEASON", args => {
			string season = args[0];
			return state => state.date.Season == season;
		});

		RegisterCondition("TIME", (start, end, state) => {
			if (start > 0 && state.timeOfDay < start)
				return false;
			if (end > 0 && state.timeOfDay > end)
				return false;
			return true;
		});

		RegisterCondition("YEAR", (value, state) =>
			state.date.Year == value
		);

		RegisterCondition("CAN_BUILD_CABIN", _ => _ =>
			Game1.getFarm().getNumberBuildingsConstructed("Cabin") < Game1.CurrentPlayerLimit - 1
		);

		RegisterCondition("CAN_BUILD_FOR_CABINS", args => _ =>
			Game1.getFarm().getNumberBuildingsConstructed(args[0]) < (Game1.getFarm().getNumberBuildingsConstructed("Cabin") + 1)
		);

		RegisterCondition("FARM_CAVE", args => _ => {
			switch (Game1.MasterPlayer.caveChoice.Value) {
				case 1:
					return args[0] == "Bats";
				case 2:
					return args[0] == "Mushrooms";
				default:
					return args[0] == "None";
			}
		});

		RegisterCondition("FARM_NAME", (name, state) =>
			state.farmer.farmName.Value == name
		);

		RegisterCondition("FARM_TYPE", args => _ =>
			Game1.GetFarmTypeID() == args[0]
		);

		RegisterCondition("IS_CUSTOM_FARM_TYPE", _ => _ =>
			Game1.whichFarm == 7 && Game1.whichModFarm != null
		);

		RegisterCondition("IS_COMMUNITY_CENTER_COMPLETE", _ => _ =>
			Game1.MasterPlayer.hasCompletedCommunityCenter() &&
			!Game1.MasterPlayer.mailReceived.Contains("JojaMember")
		);

		RegisterCondition("IS_HOST", _ => state =>
			state.farmer == Game1.MasterPlayer
		);

		RegisterCondition("IS_JOJA_MART_COMPLETE", _ => _ =>
			Game1.MasterPlayer.hasCompletedCommunityCenter() &&
			Game1.MasterPlayer.mailReceived.Contains("JojaMember")
		);

		RegisterCondition("LOCATION_ACCESSIBLE", args => _ =>
			Game1.isLocationAccessible(args[0])
		);

		RegisterCondition("LOCATION_CONTEXT", args => state => {
			switch (GetLocation(args[0], state)?.GetLocationContext()) {
				case GameLocation.LocationContext.Default:
					return args[1] == "Default";
				case GameLocation.LocationContext.Island:
					return args[1] == "Island";
				default:
					return false;
			}
		});

		RegisterCondition("LOCATION_IS_MINES", args => state =>
			GetLocation(args[0], state) is MineShaft shaft && (shaft.mineLevel < 121 || shaft.mineLevel == 77377)
		);

		RegisterCondition("LOCATION_IS_SKULL_CAVE", args => state =>
			GetLocation(args[0], state) is MineShaft shaft && shaft.mineLevel >= 121 && shaft.mineLevel != 77377
		);

		RegisterCondition("LOCATION_SEASON", args => state =>
			state.date.Season == args[1]
		);

		RegisterCondition("WEATHER", args => {
			int weather = args[1] switch {
				"Sun" => 0,
				"Rain" => 1,
				"Wind" => 2,
				"Storm" => 3,
				"Festival" => 4,
				"Snow" => 5,
				"Wedding" => 6,
				_ => -1
			};

			return state => {
				var loc = GetLocation(args[0], state);
				if (loc == null)
					return false;

				var ctx = loc.GetLocationContext();
				var wtr = Game1.netWorldState.Value.GetWeatherForLocation(ctx);

				bool wind = wtr.isDebrisWeather.Value;
				bool storm = wtr.isLightning.Value;
				bool rain = wtr.isRaining.Value;
				bool snow = wtr.isSnowing.Value;

				switch(weather) {
					case 0: // Sun
						return !wind && !storm && !rain && !snow;
					case 1: // Rain
						return rain && !storm;
					case 2: // Wind
						return wind;
					case 3: // Storm
						return storm;
					case 4: // Festival
						return ctx == GameLocation.LocationContext.Default && Utility.isFestivalDay(state.date.DayOfMonth, state.date.Season);
					case 5: // Snow
						return snow;
					case 6: // Wedding
						return ctx == GameLocation.LocationContext.Default && Game1.weddingToday;
				}

				return false;
			};
		});


		RegisterCondition("WORLD_STATE", args => state =>
			Game1.netWorldState.Value.hasWorldStateID(args[0])
		);

		RegisterCondition("MINE_LOWEST_LEVEL_REACHED", (level, state) =>
			state.farmer.deepestMineLevel >= level
		);

		RegisterCondition("PLAYER_COMBAT_LEVEL", (farmer, level, _) =>
			farmer.CombatLevel >= level
		);

		RegisterCondition("PLAYER_FARMING_LEVEL", (farmer, level, _) =>
			farmer.FarmingLevel >= level
		);

		RegisterCondition("PLAYER_FISHING_LEVEL", (farmer, level, _) =>
			farmer.FishingLevel >= level
		);

		RegisterCondition("PLAYER_FORAGING_LEVEL", (farmer, level, _) => 
			farmer.ForagingLevel >= level
		);

		RegisterCondition("PLAYER_MINING_LEVEL", (farmer, level, _) =>
			farmer.MiningLevel >= level
		);

		RegisterCondition("PLAYER_CURRENT_MONEY", (farmer, amount, _) =>
			farmer.Money >= amount
		);

		RegisterCondition("PLAYER_FARMHOUSE_UPGRADE", (farmer, level, _) =>
			farmer.HouseUpgradeLevel >= level
		);

		RegisterCondition("PLAYER_GENDER", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				(farmer.IsMale ? "Male" : "Female") == args[1]
			)
		);

		RegisterCondition("PLAYER_HAS_ACHIEVEMENT", (farmer, id, _) =>
			farmer.achievements.Contains(id)
		);

		RegisterCondition("PLAYER_HAS_ALL_ACHIEVEMENTS", args => {
			Dictionary<int, string> achievements = Game1.content.Load<Dictionary<int, string>>(@"Data\Achievements");
			int[] keys = achievements.Keys.ToArray();

			return state => CheckMatchingPlayers(args[0], state, farmer => {
				foreach (int key in keys) {
					if (!farmer.achievements.Contains(key))
						return false;
				}

				return true;
			});
		});

		RegisterCondition("PLAYER_HAS_CAUGHT_FISH", (farmer, fish, _) =>
			farmer.fishCaught.ContainsKey(fish)
		);

		RegisterCondition("PLAYER_HAS_CONVERSATION_TOPIC", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				farmer.activeDialogueEvents.ContainsKey(args[1])
			)
		);

		RegisterCondition("PLAYER_HAS_CRAFTING_RECIPE", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				farmer.craftingRecipes.ContainsKey(args[1])
			)
		);

		RegisterCondition("PLAYER_HAS_COOKING_RECIPE", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				farmer.cookingRecipes.ContainsKey(args[1])
			)
		);

		RegisterCondition("PLAYER_HAS_DIALOGUE_ANSWER", (farmer, id, _) =>
			farmer.DialogueQuestionsAnswered.Contains(id)
		);

		RegisterCondition("PLAYER_HAS_FLAG", (farmer, flag, _) =>
			farmer.hasOrWillReceiveMail(flag)
		);

		RegisterCondition("PLAYER_HAS_ITEM", (farmer, id, _) =>
			farmer.hasItemInInventory(id, 1)
		);

		RegisterCondition("PLAYER_HAS_ITEM_NAMED", (farmer, name, _) =>
			farmer.hasItemInInventoryNamed(name)
		);

		RegisterCondition("PLAYER_HAS_PROFESSION", (farmer, profession, _) =>
			farmer.professions.Contains(profession)
		);

		RegisterCondition("PLAYER_HAS_READ_LETTER", (farmer, flag, _) =>
			farmer.mailReceived.Contains(flag)
		);

		RegisterCondition("PLAYER_HAS_SECRET_NOTE", (farmer, note, _) =>
			farmer.secretNotesSeen.Contains(note)
		);

		RegisterCondition("PLAYER_HAS_SEEN_EVENT", (farmer, eventId, _) =>
			farmer.eventsSeen.Contains(eventId)
		);

		RegisterCondition("PLAYER_LOCATION_CONTEXT", args => state =>
			CheckMatchingPlayers(args[0], state, farmer => {
				switch (farmer.currentLocation?.GetLocationContext()) {
					case GameLocation.LocationContext.Default:
						return args[1] == "Default";
					case GameLocation.LocationContext.Island:
						return args[1] == "Island";
					default:
						return false;
				}
			})
		);

		RegisterCondition("PLAYER_LOCATION_NAME", (farmer, name, _) => 
			farmer.currentLocation?.Name == name
		);

		RegisterCondition("PLAYER_LOCATION_UNIQUE_NAME", (farmer, name, _) =>
			farmer.currentLocation?.NameOrUniqueName == name
		);

		RegisterCondition("PLAYER_MOD_DATA", args => {
			string? value = args.Length > 2 ?
				string.Join(' ', args[2..]) : null;
			return state => CheckMatchingPlayers(args[0], state, farmer =>
				farmer.modData.TryGetValue(args[1], out string data) &&
				(value == null || data == value)
			);
		});

		RegisterCondition("PLAYER_MONEY_EARNED", args => {
			uint amount = uint.Parse(args[1]);
			return state => CheckMatchingPlayers(args[0], state, farmer =>
				farmer.totalMoneyEarned >= amount
			);
		});

		RegisterCondition("PLAYER_HAS_CHILDREN", (farmer, count, _) =>
			farmer.getNumberOfChildren() >= count
		);

		RegisterCondition("PLAYER_HAS_PET", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				farmer.hasPet()
			)
		);

		RegisterCondition("PLAYER_HEARTS", args => {
			int level = int.Parse(args[2]);
			return state => CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.getFriendshipHeartLevelForNPC(npc.Name) >= level
				)
			);
		});

		RegisterCondition("PLAYER_HAS_MET", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.ContainsKey(npc.Name)
				)
			)
		);

		RegisterCondition("PLAYER_IS_UNMET", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					!farmer.friendshipData.ContainsKey(npc.Name)
				)
			)
		);

		RegisterCondition("PLAYER_IS_DATING", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.TryGetValue(npc.Name, out var data)
					&& data.IsDating()
				)
			)
		);

		RegisterCondition("PLAYER_IS_ENGAGED", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.TryGetValue(npc.Name, out var data)
					&& data.IsEngaged()
				)
			)
		);

		RegisterCondition("PLAYER_IS_MARRIED", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.TryGetValue(npc.Name, out var data)
					&& data.IsMarried()
				)
			)
		);

		RegisterCondition("PLAYER_IS_DIVORCED", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.TryGetValue(npc.Name, out var data)
					&& data.IsDivorced()
				)
			)
		);

		RegisterCondition("PLAYER_IS_ROOMMATE", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				CheckMatchingNPCs(args[1], state, npc =>
					farmer.friendshipData.TryGetValue(npc.Name, out var data)
					&& data.IsRoommate()
				)
			)
		);

		RegisterCondition("PLAYER_PREFERRED_PET", args => state =>
			CheckMatchingPlayers(args[0], state, farmer =>
				(farmer.catPerson ? "Cat" : "Dog") == args[1]
			)
		);

		RegisterCondition("RANDOM", args => {
			double value = double.Parse(args[0]);
			return state => state.rnd.NextDouble() < value;
		});

		RegisterCondition("PICKED_VALUE_TICK", args => {
			int min = int.Parse(args[0]);
			int max = int.Parse(args[1]);
			int value = int.Parse(args[2]);
			int offset = 0;
			if (args.Length >= 4)
				offset = int.Parse(args[3]);

			return state => new Random(state.ticks + offset).Next(min, max) == value;
		});

		RegisterCondition("PICKED_VALUE_DAYS", args => {
			int min = int.Parse(args[0]);
			int max = int.Parse(args[1]);
			int value = int.Parse(args[2]);
			int offset = 0;
			if (args.Length >= 4)
				offset = int.Parse(args[3]);

			return state => new Random(state.date.TotalDays + 1 + offset).Next(min, max) == value;
		});

		RegisterCondition("PICKED_VALUE", args => {
			int min = int.Parse(args[0]);
			int max = int.Parse(args[1]);
			int value = int.Parse(args[2]);

			return state => value == min + (int) Math.Floor(state.pickedValue * (max - min));
		});

		RegisterCondition("PICKED_VALUE_CHANCE", args => {
			double value = double.Parse(args[0]);
			return state => state.pickedValue < value;
		});

		RegisterCondition("PICKED_VALUE_SUMMER_RAIN_CHANCE", args => {
			// Is this accurate?
			double value = double.Parse(args[0]);
			double dailyBonus = double.Parse(args[1]);
			return state => state.pickedValue < (value + (state.date.DayOfMonth > 1 ? state.date.DayOfMonth * dailyBonus : 0.0));
		});

		RegisterCondition("ITEM_HAS_TAG", args => state => {
			if (state.item == null)
				return false;

			foreach (string tag in args) {
				if (!state.item.HasContextTag(tag))
					return false;
			}

			return true;
		});

		RegisterCondition("ITEM_ID", (id, state) =>
			state.item?.ParentSheetIndex == id
		);

		RegisterCondition("ITEM_QUALITY", (quality, state) =>
			state.item is SObject sobj && sobj.Quality >= quality
		);

		RegisterCondition("ITEM_STACK", (stack, state) =>
			(state.item?.Stack ?? 0) >= stack
		);
	}

	#endregion

	#region Helper Methods

	public static bool CheckMatchingPlayers(string who, GameState state, Func<Farmer, bool> func) {
		if (who == "Current")
			return func(Game1.player);
		if (who == "Host")
			return func(Game1.MasterPlayer);
		if (who == "Target")
			return func(state.farmer);
		if (who == "Any" || who == "All") {
			bool all = who == "All";
			foreach(var fmr in Game1.getAllFarmers()) {
				if (func(fmr)) {
					if (!all) {
						if (state.trace)
							state.monitor?.Log($"[GameStateQuery]    Player: {fmr.Name}", LogLevel.Trace);
						return true;
					}
				} else if (all) {
					if (state.trace)
						state.monitor?.Log($"[GameStateQuery]    Player: {fmr.Name}", LogLevel.Trace);
					return false;
				}
			}

			return all;
		}

		Farmer farmer = Game1.getFarmer(long.Parse(who));
		if (farmer == null)
			return false;

		return func(farmer);
	}

	public static bool CheckMatchingNPCs(string who, GameState state, Func<NPC, bool> func) {
		if (who == "Any" || who == "AnyDateable") {
			foreach(var n in Utility.getAllCharacters()) {
				if (who == "Any" || n.datable.Value) {
					if (func(n)) {
						if (state.trace)
							state.monitor?.Log($"[GameStateQuery]       NPC: {n.Name}", LogLevel.Trace);
						return true;
					}
				}
			}

			return false;
		}

		NPC npc = Game1.getCharacterFromName(who);
		if (npc == null)
			return false;

		return func(npc);
	}

	public static GameLocation GetLocation(string name, GameState state) {
		if (name == "Here")
			return Game1.currentLocation;
		if (name == "Target")
			return state?.location ?? state?.farmer?.currentLocation ?? Game1.currentLocation;
		return Game1.getLocationFromName(name);
	}

	#endregion

	#region Parsing

	public static ParsedQuery ParseConditions(string query, bool skip_unknown = false, bool skip_error = false, IMonitor? monitor = null) {
		if (string.IsNullOrEmpty(query))
			return ParsedQuery.EMPTY;

		List<ParsedCondition> results = new();

		if (!Initialized)
			Initialize();

		foreach(string condition in query.Split(',')) {
			string trimmed = condition.Trim();
			if (trimmed.Length == 0)
				continue;

			string[] parts = trimmed.Split(' ');
			string key = parts[0];
			bool inverted = key.StartsWith('!');
			if (inverted)
				key = key[1..];

			if (Conditions.TryGetValue(key, out var cond)) {
				Func<GameState, bool> method;
				try {
					method = cond.Invoke(parts[1..]);
				} catch (Exception ex) {
					if (skip_error) {
						monitor?.Log($"[GameStateQuery] An error occurred in condition builder for \"{trimmed}\":\n{ex}", LogLevel.Warn);
						results.Add(new ParsedCondition(trimmed, false, _ => false));
						continue;
					} else
						throw;
				}

				results.Add(new ParsedCondition(trimmed, inverted, method));

			} else if (skip_unknown) {
				monitor?.Log($"[GameStateQuery] Unknown Condition: {key}", LogLevel.Warn);
				results.Add(new ParsedCondition(trimmed, false, _ => false));

			} else
				throw new ArgumentException($"[GameStateQuery] Unknown Condition: {key}");
		}

		if (results.Count == 0)
			return ParsedQuery.EMPTY;

		return new ParsedQuery(results.ToArray());
	}

	#endregion

	#region API Compatibility

	public static bool CheckConditions(string query, Random? rnd = null, WorldDate? date = null, int? time = null, int? tick = null, double? picked = null, Farmer? who = null, GameLocation? location = null, Item? item = null, IMonitor? monitor = null, bool trace = false) {
		rnd ??= Game1.random;

		return CheckConditions(query, new GameState(
			rnd: rnd,
			date: date ?? Game1.Date,
			timeOfDay: time ?? Game1.timeOfDay,
			ticks: tick ?? Game1.ticks,
			pickedValue: picked ?? rnd.NextDouble(),
			farmer: who ?? Game1.player,
			location: location,
			item: item,
			monitor: monitor,
			trace: trace
		));
	}

	public static bool CheckConditions(string query, GameState state) {
		return ParseConditions(
			query: query,
			skip_unknown: true,
			skip_error: true,
			monitor: state.monitor
		).Evaluate(state);
	}

	#endregion

	#region Data Types

	/// <summary>
	/// GSQState represents a snapshot of game state against which conditions
	/// can be checked.
	/// </summary>
	/// <param name="rnd">The <see cref="Random"/> instance to use for Random number generation.</param>
	/// <param name="date">A <see cref="WorldDate"/> instance representing the in-game date being checked.</param>
	/// <param name="timeOfDay">The time of day, such that 600 is 6:00am, 1330 is 1:30pm, and 2600 is 2:00am.</param>
	/// <param name="ticks">The number of elapsed game ticks.</param>
	/// <param name="pickedValue">A static random value to be reused for multiple conditions.</param>
	/// <param name="farmer">The player being checked.</param>
	/// <param name="location">The location being checked.</param>
	/// <param name="item">The item being checked.</param>
	/// <param name="monitor">An optional <see cref="IMonitor"/> to use for logging.</param>
	/// <param name="trace">Whether or not trace level logging should be performed.</param>
	public record GameState(
		Random rnd,
		WorldDate date,
		int timeOfDay,
		int ticks,
		double pickedValue,
		Farmer farmer,
		GameLocation? location,
		Item? item,
		IMonitor? monitor,
		bool trace
	);

	/// <summary>
	/// A singular parsed condition.
	/// </summary>
	/// <param name="input">The string that was parsed.</param>
	/// <param name="inverted">Whether or not the result should be inverted.</param>
	/// <param name="method">The method to call to check this condition.</param>
	public record ParsedCondition(
		string input,
		bool inverted,
		Func<GameState, bool> method
	);

	/// <summary>
	/// A parsed query, made up of multiple conditions.
	/// </summary>
	public struct ParsedQuery {

		/// <summary>
		/// An empty query with no conditions. Always evaluates to true.
		/// </summary>
		public static readonly ParsedQuery EMPTY = new(null);

		/// <summary>
		/// An array of this query's conditions.
		/// </summary>
		public readonly ParsedCondition[]? Conditions;

		public ParsedQuery(ParsedCondition[]? conditions) {
			Conditions = conditions;
		}

		/// <summary>
		/// Evaluate the query with the given <paramref name="state"/>.
		/// </summary>
		/// <param name="state">The game state we're checking against.</param>
		/// <returns>Whether or not the conditions match the provided game state.</returns>
		public bool Evaluate(GameState state) {
			if (Conditions == null || Conditions.Length == 0)
				return true;

			foreach (var condition in Conditions) {
				try {
					if (state.trace)
						state.monitor?.Log($"[GameStateQuery] Condition: {condition.input}", LogLevel.Trace);

					bool result = condition.method(state);
					if (condition.inverted)
						result = !result;

					if (state.trace)
						state.monitor?.Log($"[GameStateQuery]    Result: {result}", LogLevel.Trace);

					if (!result)
						return false;

				} catch (Exception ex) {
					state.monitor?.Log($"[GameStateQuery] An error occurred in condition handler for \"{condition.input}\":\n{ex}", LogLevel.Error);
					return false;
				}
			}

			return true;
		}

		public bool Evaluate(Random? rnd = null, WorldDate? date = null, int? time = null, int? tick = null, double? picked = null, Farmer? who = null, GameLocation? location = null, Item? item = null, IMonitor? monitor = null, bool trace = false) {
			rnd ??= Game1.random;

			return Evaluate(new GameState(
				rnd: rnd,
				date: date ?? Game1.Date,
				timeOfDay: time ?? Game1.timeOfDay,
				ticks: tick ?? Game1.ticks,
				pickedValue: picked ?? rnd.NextDouble(),
				farmer: who ?? Game1.player,
				location: location,
				item: item,
				monitor: monitor,
				trace: trace
			));
		}
	};

	#endregion
}
