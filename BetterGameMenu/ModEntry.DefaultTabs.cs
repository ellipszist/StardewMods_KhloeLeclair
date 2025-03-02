using Leclair.Stardew.BetterGameMenu.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;

public partial class ModEntry {

	internal enum TabIcon {
		Inventory = 0,
		Skills = 1,
		Social = 2,
		Map = 3,
		Crafting = 4,
		Collections = 5,
		Options = 6,
		Exit = 7
	}

	internal static (IBetterGameMenuApi.DrawDelegate DrawMethod, bool DrawBackground) GetDefaultIcon(TabIcon tab) {
		var draw = ModAPI.CreateDrawImpl(Game1.mouseCursors, new Rectangle((int) tab * 16, 368, 16, 16), 4f);
		return (draw, false);
	}

	internal void RegisterDefaultTabs() {
		RegisterInventoryTab();
		RegisterSkillsTab();
		RegisterSocialTab();
		RegisterMapTab();
		RegisterCraftingTab();
		RegisterAnimalsTab();
		RegisterPowersTab();
		RegisterCollectionsTab();
		RegisterOptionsTab();
		RegisterExitTab();
	}

	internal void RegisterInventoryTab() {
		static IClickableMenu CreateInstance(IClickableMenu menu) {
			return new InventoryPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Inventory), new TabDefinition(
			Order: (int) VanillaTabOrders.Inventory,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Inventory"),
			GetIcon: () => GetDefaultIcon(TabIcon.Inventory)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: null,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterSkillsTab() {
		static IClickableMenu CreateInstance(IClickableMenu menu) {
			return new SkillsPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		static void Draw(SpriteBatch batch, Rectangle bounds) {
			Game1.player.FarmerRenderer.drawMiniPortrat(
				batch,
				position: new Vector2(bounds.X + 8, bounds.Y + 12),
				layerDepth: 0.00011f,
				scale: 3f,
				facingDirection: 2,
				who: Game1.player
			);
		}

		AddTab(nameof(VanillaTabOrders.Skills), new TabDefinition(
			Order: (int) VanillaTabOrders.Skills,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Skills"),
			GetIcon: () => (Draw, true) // GetDefaultIcon(TabIcon.Skills)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? 64 : 0),
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterSocialTab() {
		static SocialPage CreateInstance(IClickableMenu menu) {
			return new SocialPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Social), new TabDefinition(
			Order: (int) VanillaTabOrders.Social,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Social"),
			GetIcon: () => GetDefaultIcon(TabIcon.Social)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width + 36,
			GetHeight: null,
			OnResize: input => {
				var result = CreateInstance(input.Menu);
				result.postWindowSizeChange(input.OldPage);
				return result;
			},
			OnClose: null
		));
	}

	internal void RegisterMapTab() {
		static MapPage CreateInstance(IClickableMenu menu) {
			return new MapPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Map), new TabDefinition(
			Order: (int) VanillaTabOrders.Map,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Map"),
			GetIcon: () => GetDefaultIcon(TabIcon.Map)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: () => true,
			GetWidth: width => width + 128,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterCraftingTab() {
		static CraftingPage CreateInstance(IClickableMenu menu) {
			return new CraftingPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Crafting), new TabDefinition(
			Order: (int) VanillaTabOrders.Crafting,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Crafting"),
			GetIcon: () => GetDefaultIcon(TabIcon.Crafting)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: null,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterAnimalsTab() {
		static AnimalPage CreateInstance(IClickableMenu menu) {
			return new AnimalPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Animals), new TabDefinition(
			Order: (int) VanillaTabOrders.Animals,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\1_6_Strings:GameMenu_Animals"),
			GetIcon: () => (ModAPI.CreateDrawImpl(Game1.mouseCursors_1_6, new Rectangle(257, 246, 16, 16), 4f), false)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width - 64 - 16,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterPowersTab() {
		static PowersTab CreateInstance(IClickableMenu menu) {
			return new PowersTab(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Powers), new TabDefinition(
			Order: (int) VanillaTabOrders.Powers,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\1_6_Strings:GameMenu_Powers"),
			GetIcon: () => (ModAPI.CreateDrawImpl(Game1.mouseCursors_1_6, new Rectangle(216, 494, 16, 16), 4f), false)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width - 64 - 16,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

	internal void RegisterCollectionsTab() {
		static CollectionsPage CreateInstance(IClickableMenu menu) {
			return new CollectionsPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Collections), new TabDefinition(
			Order: (int) VanillaTabOrders.Collections,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Collections"),
			GetIcon: () => GetDefaultIcon(TabIcon.Collections)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width - 64 - 16,
			GetHeight: null,
			OnResize: input => {
				var result = CreateInstance(input.Menu);
				result.postWindowSizeChange(input.OldPage);
				return result;
			},
			OnClose: null
		));
	}

	internal void RegisterOptionsTab() {
		static OptionsPage CreateInstance(IClickableMenu menu) {
			return new OptionsPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Options), new TabDefinition(
			Order: (int) VanillaTabOrders.Options,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Options"),
			GetIcon: () => GetDefaultIcon(TabIcon.Options)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => {
				int extraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 96 : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? 192 : 0));
				return width + extraWidth;
			},
			GetHeight: null,
			OnResize: input => {
				var result = CreateInstance(input.Menu);
				result.postWindowSizeChange(input.OldPage);
				return result;
			},
			OnClose: null
		));
	}

	internal void RegisterExitTab() {
		static ExitPage CreateInstance(IClickableMenu menu) {
			return new ExitPage(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);
		}

		AddTab(nameof(VanillaTabOrders.Exit), new TabDefinition(
			Order: (int) VanillaTabOrders.Exit,
			GetDisplayName: () => Game1.content.LoadString(@"Strings\UI:GameMenu_Exit"),
			GetIcon: () => GetDefaultIcon(TabIcon.Exit)
		), new TabImplementationDefinition(
			Source: "stardew",
			Priority: 0,
			GetPageInstance: CreateInstance,
			GetDecoration: null,
			GetTabVisible: null,
			GetMenuInvisible: null,
			GetWidth: width => width - 64 - 16,
			GetHeight: null,
			OnResize: input => CreateInstance(input.Menu),
			OnClose: null
		));
	}

}
