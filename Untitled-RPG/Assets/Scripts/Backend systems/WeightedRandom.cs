using System;
using System.Collections.Generic;

[System.Serializable]
public class WeightedRandomBag<T>  {

    [System.Serializable]
    public struct WeightedEntry {
        public T item;
        public double weight;
    }

    private struct Entry {
        public double accumulatedWeight;
        public T item;
    }

    private List<Entry> entries = new List<Entry>();
    private double accumulatedWeight;
    private Random rand = new Random();

    public List<WeightedEntry> WeightedEntries = new List<WeightedEntry>();

    public double totalWeight {
        get {
            if (WeightedEntries.Count <= 0) return 0;
            
            double tw = 0; 
            foreach (WeightedEntry we in WeightedEntries) tw += we.weight;
            return tw;
        }
    }

    private void AddEntry(T item, double weight) {
        accumulatedWeight += weight;
        entries.Add(new Entry { item = item, accumulatedWeight = accumulatedWeight });
    }

    public void Initialize () {
        foreach (WeightedEntry en in WeightedEntries) AddEntry(en.item, en.weight);
    }

    public T GetRandom() {
        if (entries.Count == 0) Initialize();
        
        double r = rand.NextDouble() * accumulatedWeight;

        foreach (Entry entry in entries) {
            if (entry.accumulatedWeight >= r) {
                return entry.item;
            }
        }
        return default(T); //should only happen when there are no entries
    }
}