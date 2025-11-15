using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DynamicLODManager : MonoBehaviour
{
    [Header("Performance Target")]
    [SerializeField] [Range(24, 200)] private int targetFPS = 60;
    
    [Header("Adjustment Settings")]
    [SerializeField] [Range(0.001f, 0.1f)] private float adjustmentSpeed = 0.01f;
    [SerializeField] [Range(0.1f, 5.0f)] private float updateInterval = 1.0f;
    
    [Header("LOD Maximum Percentages")]
    [SerializeField] [Range(0f, 1f)] private float maxLOD0Percentage = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float maxLOD1Percentage = 0.99f;
    [SerializeField] [Range(0f, 1f)] private float maxLOD2Percentage = 0.98f;
    [SerializeField] [Range(0f, 1f)] private float maxCullingPercentage = 0.97f;
    
    [Header("LOD Minimum Percentages")]
    [SerializeField] [Range(0f, 1f)] private float minLOD0Percentage = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float minLOD1Percentage = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float minLOD2Percentage = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float minCullingPercentage = 0.0f;
    
    private const int FPS_HISTORY_SIZE = 60;
    private const float FPS_THRESHOLD_LOW = -5f;
    private const float FPS_THRESHOLD_HIGH = 10f;
    
    private const float MIN_ADJUSTMENT_SPEED = 0.001f;
    private const float MAX_ADJUSTMENT_SPEED = 0.1f;
    private const float MIN_UPDATE_INTERVAL = 0.1f;
    private const float MAX_UPDATE_INTERVAL = 5.0f;
    
    private const float ABSOLUTE_MIN_PERCENTAGE = 0.0f;
    private const float ABSOLUTE_MAX_PERCENTAGE = 1.0f;
    private const float TRANSITION_GAP = 0.01f;
    private const float ZERO_THRESHOLD = 0.01f;
    
    private float growthFactor;
    private float shrinkFactor;
    private float lodAddAmount;
    private float lodShrinkAmount;
    
    private class LODGroupData
    {
        public DynamicLOD dynamicLOD;
        public LODGroup lodGroup;
        public float[] originalTransitions;
        public float[] currentTransitions;
        public float[] addedOffsets;
        public float currentMultiplier = 1.0f;
    }
    
    private List<LODGroupData> lodGroupsData = new List<LODGroupData>();
    private float fpsCheckTimer = 0f;
    private float averageFPS = 0f;
    private List<float> fpsHistory = new List<float>();

    private void Awake()
    {
        Application.targetFrameRate = targetFPS;
        ClampSerializedFields();
        CalculateAdjustmentFactors();
        ScanSceneForLODGroups();
    }
    
    private void ClampSerializedFields()
    {
        adjustmentSpeed = Mathf.Clamp(adjustmentSpeed, MIN_ADJUSTMENT_SPEED, MAX_ADJUSTMENT_SPEED);
        updateInterval = Mathf.Clamp(updateInterval, MIN_UPDATE_INTERVAL, MAX_UPDATE_INTERVAL);
        
        maxLOD0Percentage = Mathf.Clamp(maxLOD0Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        maxLOD1Percentage = Mathf.Clamp(maxLOD1Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        maxLOD2Percentage = Mathf.Clamp(maxLOD2Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        maxCullingPercentage = Mathf.Clamp(maxCullingPercentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        
        minLOD0Percentage = Mathf.Clamp(minLOD0Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        minLOD1Percentage = Mathf.Clamp(minLOD1Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        minLOD2Percentage = Mathf.Clamp(minLOD2Percentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
        minCullingPercentage = Mathf.Clamp(minCullingPercentage, ABSOLUTE_MIN_PERCENTAGE, ABSOLUTE_MAX_PERCENTAGE);
    }
    
    private void CalculateAdjustmentFactors()
    {
        growthFactor = 1.0f + adjustmentSpeed;
        shrinkFactor = 1.0f - (adjustmentSpeed * 0.5f);
        lodAddAmount = adjustmentSpeed;
        lodShrinkAmount = adjustmentSpeed * 0.5f;
    }
    
    void Update()
    {
        if (Time.unscaledDeltaTime > 0)
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            fpsHistory.Add(currentFPS);
            if (fpsHistory.Count > FPS_HISTORY_SIZE) fpsHistory.RemoveAt(0);
            averageFPS = fpsHistory.Average();
        }
        
        fpsCheckTimer += Time.unscaledDeltaTime;
        
        if (fpsCheckTimer >= updateInterval && lodGroupsData.Count > 0)
        {
            fpsCheckTimer = 0f;
            AdjustLODs();
        }
    }
    
    private void AdjustLODs()
    {
        float fpsDiff = averageFPS - targetFPS;
        
        if (fpsDiff < FPS_THRESHOLD_LOW)
        {
            foreach (var data in lodGroupsData)
            {
                // Check if enableDynamic is true before adjusting
                if (data.dynamicLOD == null || !data.dynamicLOD.enableDynamic) continue;
                if (data.lodGroup == null || data.originalTransitions == null) continue;
                
                data.currentMultiplier *= growthFactor;
                
                LOD[] lods = data.lodGroup.GetLODs();
                
                for (int i = 0; i < lods.Length && i < data.originalTransitions.Length; i++)
                {
                    float newValue;
                    float minForThisLOD = GetMinForLODIndex(i, lods.Length);
                    float maxForThisLOD = GetMaxForLODIndex(i, lods.Length);
                    
                    // If original value is near zero, use addition instead of multiplication
                    if (data.originalTransitions[i] < ZERO_THRESHOLD)
                    {
                        data.addedOffsets[i] += lodAddAmount;
                        data.addedOffsets[i] = Mathf.Min(data.addedOffsets[i], maxForThisLOD);
                        newValue = data.originalTransitions[i] + data.addedOffsets[i];
                    }
                    else
                    {
                        // Original is not zero, use multiplication
                        newValue = data.originalTransitions[i] * data.currentMultiplier;
                    }
                    
                    data.currentTransitions[i] = Mathf.Clamp(newValue, minForThisLOD, maxForThisLOD);
                }
                
                // Enforce descending order
                for (int i = 0; i < lods.Length; i++)
                {
                    if (i > 0 && data.currentTransitions[i] >= data.currentTransitions[i - 1])
                    {
                        data.currentTransitions[i] = data.currentTransitions[i - 1] - TRANSITION_GAP;
                        data.currentTransitions[i] = Mathf.Max(data.currentTransitions[i], GetMinForLODIndex(i, lods.Length));
                    }
                    
                    lods[i].screenRelativeTransitionHeight = data.currentTransitions[i];
                }
                
                data.lodGroup.SetLODs(lods);
            }
        }
        else if (fpsDiff > FPS_THRESHOLD_HIGH)
        {
            foreach (var data in lodGroupsData.Where(d => d.currentMultiplier > 1.0f || d.addedOffsets.Any(o => o > 0f)))
            {
                // Check if enableDynamic is true before adjusting
                if (data.dynamicLOD == null || !data.dynamicLOD.enableDynamic) continue;
                if (data.lodGroup == null || data.originalTransitions == null) continue;
                
                data.currentMultiplier *= shrinkFactor;
                data.currentMultiplier = Mathf.Max(data.currentMultiplier, 1.0f);
                
                LOD[] lods = data.lodGroup.GetLODs();
                
                for (int i = 0; i < lods.Length && i < data.originalTransitions.Length; i++)
                {
                    float newValue;
                    float minForThisLOD = GetMinForLODIndex(i, lods.Length);
                    float maxForThisLOD = GetMaxForLODIndex(i, lods.Length);
                    
                    if (data.originalTransitions[i] < ZERO_THRESHOLD)
                    {
                        data.addedOffsets[i] -= lodShrinkAmount;
                        data.addedOffsets[i] = Mathf.Max(data.addedOffsets[i], 0f);
                        newValue = data.originalTransitions[i] + data.addedOffsets[i];
                    }
                    else
                    {
                        newValue = data.originalTransitions[i] * data.currentMultiplier;
                    }
                    
                    data.currentTransitions[i] = Mathf.Clamp(newValue, minForThisLOD, maxForThisLOD);
                    
                    lods[i].screenRelativeTransitionHeight = data.currentTransitions[i];
                }
                
                data.lodGroup.SetLODs(lods);
            }
        }
    }
    
    private float GetMaxForLODIndex(int lodIndex, int totalLODs)
    {
        if (lodIndex == totalLODs - 1)
        {
            return maxCullingPercentage;
        }
        else if (lodIndex == totalLODs - 2)
        {
            return maxLOD2Percentage;
        }
        else if (lodIndex == totalLODs - 3)
        {
            return maxLOD1Percentage;
        }
        else
        {
            return maxLOD0Percentage;
        }
    }
    
    private float GetMinForLODIndex(int lodIndex, int totalLODs)
    {
        if (lodIndex == totalLODs - 1)
        {
            return minCullingPercentage;
        }
        else if (lodIndex == totalLODs - 2)
        {
            return minLOD2Percentage;
        }
        else if (lodIndex == totalLODs - 3)
        {
            return minLOD1Percentage;
        }
        else
        {
            return minLOD0Percentage;
        }
    }
    
    public void ScanSceneForLODGroups()
    {
        lodGroupsData.Clear();
        DynamicLOD[] foundDynamicLODs = FindObjectsOfType<DynamicLOD>();
        
        foreach (var dynamicLOD in foundDynamicLODs)
        {
            // Only add if enableDynamic is true
            if (!dynamicLOD.enableDynamic) continue;
            
            LODGroup lodGroup = dynamicLOD.LODGroup;
            if (lodGroup == null) continue;
            
            LOD[] lods = lodGroup.GetLODs();
            if (lods.Length == 0) continue;
            
            var data = new LODGroupData
            {
                dynamicLOD = dynamicLOD,
                lodGroup = lodGroup,
                originalTransitions = new float[lods.Length],
                currentTransitions = new float[lods.Length],
                addedOffsets = new float[lods.Length],
                currentMultiplier = 1.0f
            };
            
            for (int i = 0; i < lods.Length; i++)
            {
                data.originalTransitions[i] = lods[i].screenRelativeTransitionHeight;
                data.currentTransitions[i] = lods[i].screenRelativeTransitionHeight;
                data.addedOffsets[i] = 0f;
            }
            
            lodGroupsData.Add(data);
        }
        
        Debug.Log($"DynamicLODManager: Found {lodGroupsData.Count} DynamicLOD components with enableDynamic=true");
    }
    
    public void ResetAllLODGroups()
    {
        foreach (var data in lodGroupsData)
        {
            if (data.lodGroup == null || data.originalTransitions == null) continue;
            
            LOD[] lods = data.lodGroup.GetLODs();
            
            for (int i = 0; i < lods.Length && i < data.originalTransitions.Length; i++)
            {
                lods[i].screenRelativeTransitionHeight = data.originalTransitions[i];
                data.currentTransitions[i] = data.originalTransitions[i];
                data.addedOffsets[i] = 0f;
            }
            
            data.currentMultiplier = 1.0f;
            data.lodGroup.SetLODs(lods);
        }
    }
}