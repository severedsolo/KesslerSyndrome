using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace KesslerSyndrome
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Kessler : MonoBehaviour
    {
        DateTime nextTick = DateTime.MinValue;
        double lastChecked = 0;
        double impactTimer = 0;
        bool spawned;
        System.Random r = new System.Random();
        bool showGUI = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 240, 50);

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
        }

        private void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            showGUI = false;
        }

        void Update()
        {
            if (FlightGlobals.ActiveVessel.altitude < FlightGlobals.ActiveVessel.mainBody.atmosphereDepth) return;
            if (DateTime.Now < nextTick) return;
            nextTick = DateTime.Now.AddSeconds(30);
            if (impactTimer > 0)
            {
                double timeSinceLastTick = Planetarium.GetUniversalTime() - lastChecked;
                lastChecked = Planetarium.GetUniversalTime();
                impactTimer = impactTimer - timeSinceLastTick;
                return;
            }
            if (spawned) DebrisTrigger(20);
            int spawn = 0;
            spawn = SpawnChance();
            if (r.Next(1, 100) > spawn || spawned) return;
            impactTimer = r.Next(1, 300);
            Debug.Log("[KesslerSyndrome] Added new Debris Cloud");
            spawned = true;              
        }

        int SpawnChance()
        {
            if (!FlightGlobals.ready) return 0;
            Vessel active = FlightGlobals.ActiveVessel;
            if (active == null) return 0;
            CelestialBody SOI = active.mainBody;
            double minAltitude = SOI.atmosphereDepth;
            if (minAltitude < 5000) minAltitude = 5000;
            if (active.altitude < minAltitude) return 0;
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels;
            float debris = 0.0f;
            if (vessels.Count() == 0) return 0;
            for(int i = 0; i < vessels.Count(); i++)
            {
                Vessel v = vessels.ElementAt(i);
                if (v.vesselType == VesselType.Debris && v.mainBody == SOI && v.orbit.ApA > active.altitude && v.orbit.PeA > minAltitude) debris = debris + 1.0f;
            }
            int CloudChance = HighLogic.CurrentGame.Parameters.CustomParams<KesslerSettings>().CloudChance;
            float chance = (debris / CloudChance)*100.0f;
            return (int)chance;
        }

        void DebrisTrigger(int chance)
        {
            int i = r.Next(1, 100);
            if (i > chance)
            {
                spawned = false;
                return;
            }
            List<Part> parts = FlightGlobals.ActiveVessel.parts;
            if (parts.Count == 0) return;
            bool partFound = false;
            while (!partFound)
            {
                i = r.Next(parts.Count);
                Part destroyed = parts.ElementAt(i);
                if (destroyed.ShieldedFromAirstream) continue;
                partFound = true;
                destroyed.explode();
            }
            Debug.Log("[KesslerSyndrome] Debris impact!");
            ScreenMessages.PostScreenMessage("Micrometeoroid Impact Detected!");
            spawned = false;
        }
        public void OnGUI()
        {
            if (showGUI)
            {

                Window = GUILayout.Window(2354856, Window, GUIDisplay, "Kessler Syndrome", GUILayout.Width(200));
            }
        }

        public void GUIReady()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            if (ToolbarButton == null)
            {
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT, GameDatabase.Instance.GetTexture("KesslerSyndrome/Icon", false));
            }
        }

        public void GUISwitch()
        {
            if (showGUI)
            {
                showGUI = false;
            }
            else
            {
                showGUI = true;
            }
        }

        void GUIDisplay(int windowID)
        {
            GUILayout.Label("Chance of Debris cloud " + (SpawnChance()) + "%");
            GUI.DragWindow();
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }
    }
}
