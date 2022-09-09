﻿namespace SMPL
{
	public static class Event
	{
		public delegate void EventHandler();
		public delegate void ThingEventHandler(string thingUID);
		public delegate void GUIButtonEventHandler(string guiUID, Thing.GUI.ButtonDetails buttonDetails);
		public delegate void SceneEventHandler(string sceneName);
		public delegate void MultiplayerClientEventHandler(string clientUniqueID);
		public delegate void MultiplayerMessageEventHandler(LAN.Message message);
		public delegate void ParticleEventHandler(string particleManagerUID, Thing.Particle particle);
		public delegate void ListItemEventHandler(string listUID, int itemIndex, Thing.GUI.ListItem item);

		public static event MultiplayerClientEventHandler MultiplayerClientConnected;
		public static event MultiplayerClientEventHandler MultiplayerClientDisconnected;
		public static event MultiplayerClientEventHandler MultiplayerClientTakenUID;
		public static event EventHandler MultiplayerServerStarted;
		public static event EventHandler MultiplayerServerStopped;
		public static event MultiplayerMessageEventHandler MultiplayerMessageReceived;

		public static event EventHandler GameStopped;

		public static event SceneEventHandler SceneStarted;
		public static event SceneEventHandler SceneUpdated;
		public static event SceneEventHandler SceneStopped;

		public static event ThingEventHandler ThingCreated;

		public static event ThingEventHandler ButtonClicked;
		public static event ThingEventHandler ButtonHeld;
		public static event ThingEventHandler ButtonHovered;
		public static event ThingEventHandler ButtonUnhovered;
		public static event ThingEventHandler ButtonPressed;
		public static event ThingEventHandler ButtonReleased;
		public static event ThingEventHandler ButtonDragged;
		public static event ThingEventHandler ButtonDropped;

		public static event GUIButtonEventHandler ScrollBarButtonClicked;
		public static event GUIButtonEventHandler ScrollBarButtonHeld;
		public static event GUIButtonEventHandler ScrollBarButtonHovered;
		public static event GUIButtonEventHandler ScrollBarButtonUnhovered;
		public static event GUIButtonEventHandler ScrollBarButtonPressed;
		public static event GUIButtonEventHandler ScrollBarButtonReleased;
		public static event GUIButtonEventHandler ScrollBarScrolled;

		public static event ListItemEventHandler ListItemClicked;
		public static event ListItemEventHandler ListItemHeld;
		public static event ListItemEventHandler ListItemHovered;
		public static event ListItemEventHandler ListItemUnhovered;
		public static event ListItemEventHandler ListItemPressed;
		public static event ListItemEventHandler ListItemReleased;
		public static event ListItemEventHandler ListItemSelected;
		public static event ListItemEventHandler ListItemDeselected;

		public static event GUIButtonEventHandler DropdownListButtonClicked;
		public static event GUIButtonEventHandler DropdownListButtonHovered;
		public static event GUIButtonEventHandler DropdownListButtonUnhovered;
		public static event GUIButtonEventHandler DropdownListButtonPressed;
		public static event GUIButtonEventHandler DropdownListButtonReleased;
		public static event ThingEventHandler DropdownListToggled;

		public static event ThingEventHandler CheckboxChecked;
		public static event ThingEventHandler InputboxSubmitted;

		public static event ParticleEventHandler ParticleUpdated;

		#region Backend
		internal static void GameStop() => GameStopped?.Invoke();

		internal static void SceneStart(string name) => SceneStarted?.Invoke(name);
		internal static void SceneUpdate(string name) => SceneUpdated?.Invoke(name);
		internal static void SceneStop(string name) => SceneStopped?.Invoke(name);

		internal static void ThingCreate(string uid) => ThingCreated?.Invoke(uid);

		internal static void ButtonClick(string uid) => ButtonClicked?.Invoke(uid);
		internal static void ButtonHold(string uid) => ButtonHeld?.Invoke(uid);
		internal static void ButtonHover(string uid) => ButtonHovered?.Invoke(uid);
		internal static void ButtonUnhover(string uid) => ButtonUnhovered?.Invoke(uid);
		internal static void ButtonPress(string uid) => ButtonPressed?.Invoke(uid);
		internal static void ButtonRelease(string uid) => ButtonReleased?.Invoke(uid);
		internal static void ButtonDrag(string uid) => ButtonDragged?.Invoke(uid);
		internal static void ButtonDrop(string uid) => ButtonDropped?.Invoke(uid);

		internal static void ListItemClick(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemClicked?.Invoke(listUID, itemIndex, item);
		internal static void ListItemHold(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemHeld?.Invoke(listUID, itemIndex, item);
		internal static void ListItemHover(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemHovered?.Invoke(listUID, itemIndex, item);
		internal static void ListItemUnhover(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemUnhovered?.Invoke(listUID, itemIndex, item);
		internal static void ListItemPress(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemPressed?.Invoke(listUID, itemIndex, item);
		internal static void ListItemRelease(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemReleased?.Invoke(listUID, itemIndex, item);
		internal static void ListItemSelect(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemSelected?.Invoke(listUID, itemIndex, item);
		internal static void ListItemDeselect(string listUID, int itemIndex, Thing.GUI.ListItem item) => ListItemDeselected?.Invoke(listUID, itemIndex, item);

		internal static void DropdownListButtonClick(string uid, Thing.GUI.ButtonDetails btn) => DropdownListButtonClicked?.Invoke(uid, btn);
		internal static void DropdownListButtonHover(string uid, Thing.GUI.ButtonDetails btn) => DropdownListButtonHovered?.Invoke(uid, btn);
		internal static void DropdownListButtonUnhover(string uid, Thing.GUI.ButtonDetails btn) => DropdownListButtonUnhovered?.Invoke(uid, btn);
		internal static void DropdownListButtonPress(string uid, Thing.GUI.ButtonDetails btn) => DropdownListButtonPressed?.Invoke(uid, btn);
		internal static void DropdownListButtonRelease(string uid, Thing.GUI.ButtonDetails btn) => DropdownListButtonReleased?.Invoke(uid, btn);
		internal static void DropdownListToggle(string uid) => DropdownListToggled?.Invoke(uid);

		internal static void ScrollBarButtonClick(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonClicked?.Invoke(uid, btn);
		internal static void ScrollBarButtonHold(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonHeld?.Invoke(uid, btn);
		internal static void ScrollBarButtonHover(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonHovered?.Invoke(uid, btn);
		internal static void ScrollBarButtonUnhover(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonUnhovered?.Invoke(uid, btn);
		internal static void ScrollBarButtonPress(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonPressed?.Invoke(uid, btn);
		internal static void ScrollBarButtonRelease(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarButtonReleased?.Invoke(uid, btn);
		internal static void ScrollBarScroll(string uid, Thing.GUI.ButtonDetails btn) => ScrollBarScrolled?.Invoke(uid, btn);

		internal static void CheckboxCheck(string uid) => CheckboxChecked?.Invoke(uid);
		internal static void InputboxSubmit(string uid) => InputboxSubmitted?.Invoke(uid);

		internal static void ParticleUpdate(string uid, Thing.Particle particle) => ParticleUpdated?.Invoke(uid, particle);

		internal static void MultiplayerClientConnect(string clientUID)
		{
			MultiplayerClientConnected?.Invoke(clientUID);
		}
		internal static void MultiplayerClientDisconnect(string clientUID)
		{
			MultiplayerClientDisconnected?.Invoke(clientUID);
		}
		internal static void MultiplayerClientTakeUID(string clientUID)
		{
			MultiplayerClientTakenUID?.Invoke(clientUID);
		}
		internal static void MultiplayerServerStart()
		{
			MultiplayerServerStarted?.Invoke();
		}
		internal static void MultiplayerServerStop()
		{
			MultiplayerServerStopped?.Invoke();
		}
		internal static void MultiplayerMessageReceive(LAN.Message message)
		{
			MultiplayerMessageReceived?.Invoke(message);
		}
		#endregion
	}
}
