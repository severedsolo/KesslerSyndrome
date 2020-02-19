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
        bool spawned;
        System.Random r = new System.Random();
        bool showGUI = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 240, 50);
        bool paused = false;
        bool debug = false;

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            Debug.Log("[KesslerSyndrome]: Kessler is awake");
        }

        private void onGameUnpause()
        {
            paused = false;
        }

        private void onGamePause()
        {
            paused = true;
        }

        private void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            showGUI = false;
            Debug.Log("[KesslerSyndrome]: Toolbar button removed");
        }

        void Update()
        {
            if (FlightGlobals.ActiveVessel.altitude < FlightGlobals.ActiveVessel.mainBody.atmosphereDepth) return;
            if (FlightGlobals.ActiveVessel.altitude > FlightGlobals.ActiveVessel.mainBody.scienceValues.spaceAltitudeThreshold) return;
            if (DateTime.Now < nextTick) return;
            nextTick = DateTime.Now.AddSeconds(30);
            if (paused) return;
            if (spawned) DebrisTrigger(20);
            int spawn = 0;
            spawn = SpawnChance();
            int spawnRoll = r.Next(1, 100);
            if (spawnRoll > spawn) return;
            spawned = true;              
        }

        int SpawnChance()
        {
            if (!FlightGlobals.ready) return 0;
            Vessel active = FlightGlobals.ActiveVessel;
            if (active == null) return 0;
            CelestialBody SOI = active.mainBody;
            double activeInclination = active.orbit.inclination;
            bool activeIsRetrograde = active.orbit.inclination > 90;
            if (activeIsRetrograde) activeInclination = 180.0f - activeInclination;
            double minAltitude = SOI.atmosphereDepth;
            double minInclination = activeInclination - 15.0f;
            double maxInclination = activeInclination + 15.0f;
            if (minAltitude < 5000) minAltitude = 5000;
            if (active.altitude < minAltitude) return 0;
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels;
            float debris = 0.0f;
            if (vessels.Count() == 0) return 0;
            for(int i = 0; i < vessels.Count(); i++)
            {
                Vessel v = vessels.ElementAt(i);
                if (v == active) continue;
                if (v.vesselType == VesselType.Debris && v.mainBody == SOI && v.orbit.ApA > active.altitude && v.orbit.PeA > minAltitude)
                {
                    if (v.orbit.PeA > v.mainBody.scienceValues.spaceAltitudeThreshold) continue;
                    bool debrisIsRetrograde = v.orbit.inclination > 90;
                    bool retroCheck = false;
                    if (activeIsRetrograde && !debrisIsRetrograde) retroCheck = true;
                    else if (!activeIsRetrograde && debrisIsRetrograde) retroCheck = true;
                    if (v.orbit.inclination > minInclination && v.orbit.inclination < maxInclination) debris = debris + 1.0f;
                    else if ((180.0f - v.orbit.inclination) > minInclination && (180.0f - v.orbit.inclination < maxInclination)) debris = debris + 1.0f;
                    else continue;
                    if ((active.orbit.inclination + 15) > v.orbit.inclination && (active.orbit.inclination - 15) < v.orbit.inclination) retroCheck = false;
                    if (retroCheck) debris = debris + 1.0f;
                }
            }
            int CloudChance = HighLogic.CurrentGame.Parameters.CustomParams<KesslerSettings>().CloudChance;
            float chance = (debris / CloudChance)*100.0f;
            if (debug) Debug.Log("[KesslerSyndrome]: Spawn Chance is " +(int)chance +"%");
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
            Part destroyed = parts.ElementAt(0);
            while (!partFound)
            {
                i = r.Next(parts.Count);
                destroyed = parts.ElementAt(i);
                if (destroyed.ShieldedFromAirstream) continue;
                partFound = true;
                destroyed.explode();
            }
            Debug.Log("[KesslerSyndrome] Debris impact!");
            ScreenMessages.PostScreenMessage("Micrometeoroid Impact Detected! "+destroyed.name+" has been destroyed");
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
                Debug.Log("[KesslerSyndrome]: Toolbar Button added");
            }
        }

        public void GUISwitch()
        {
            if (showGUI)
            {
                showGUI = false;
                Debug.Log("[KesslerSyndrome]: GUI turned off");
            }
            else
            {
                showGUI = true;
                Debug.Log("[KesslerSyndrome]: GUI turned on");
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
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
            Debug.Log("[KesslerSyndrome]: Kessler destroyed");
        }
    }
}
