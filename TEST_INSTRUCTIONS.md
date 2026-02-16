# MP3 Loading Test Instructions

## Overview
This document provides step-by-step instructions for testing the MP3 loading functionality in the Desert Rider Unity project.

## Test File Information
- **File Path:** `E:\Downloads\Heavy Is The Crown (Original Score).mp3`
- **File Size:** 1.6 MB
- **Status:** File verified and exists

## Prerequisites
1. Unity Editor installed (2021.3 or later recommended)
2. NAudio.dll plugin present in `Assets/Plugins/`
3. Test MP3 file accessible at the specified path
4. All required scripts compiled without errors

## Required Files Checklist
All required files have been verified:
- ✓ `Assets/Scripts/MP3/MP3Loader.cs` - Core MP3 loading functionality
- ✓ `Assets/Scripts/Testing/MP3LoadTest.cs` - Test script
- ✓ `Assets/Scripts/UI/WaveformVisualizer.cs` - Waveform visualization
- ✓ `Assets/Scenes/MP3LoadTest.unity` - Test scene (configured with test file path)
- ✓ `Assets/Plugins/NAudio.dll` - NAudio library for MP3 decoding

## Testing Steps

### Step 1: Open the Project
1. Launch Unity Hub
2. Open the Desert Rider project from: `c:\Users\svenftw\OneDrive\claude_code_vscode-moto-game`
3. Wait for Unity to import all assets and compile scripts

### Step 2: Open the Test Scene
1. In the Project window, navigate to `Assets/Scenes/`
2. Double-click on `MP3LoadTest.unity` to open the scene
3. Verify the scene loads without errors

### Step 3: Verify Scene Configuration
1. In the Hierarchy window, locate the `MP3LoadTest` GameObject
2. Select it to view properties in the Inspector
3. Verify the `MP3LoadTest` component is attached
4. Check that `mp3FilePath` shows: `E:\Downloads\Heavy Is The Crown (Original Score).mp3`
5. If the path is incorrect, manually update it in the Inspector

### Step 4: Run the Test
1. Click the Play button at the top of the Unity Editor
2. The test will automatically execute when the scene starts
3. Monitor the Console window for output messages

### Step 5: Verify Results
Check the Console window for the following information:
- MP3 file loading status
- Audio properties (duration, sample rate, channel count)
- Sample data information
- Waveform generation status
- Any error or warning messages

## Expected Output

### Success Indicators
When the test runs successfully, you should see console output similar to:
```
[MP3LoadTest] Starting MP3 load test...
[MP3LoadTest] Loading file: E:\Downloads\Heavy Is The Crown (Original Score).mp3
[MP3Loader] Starting to load MP3 file...
[MP3Loader] Successfully loaded MP3 file
[MP3Loader] Duration: XX.XX seconds
[MP3Loader] Sample Rate: 44100 Hz
[MP3Loader] Channels: 2
[MP3Loader] Total Samples: XXXXXX
[MP3LoadTest] Load completed successfully
[WaveformVisualizer] Waveform data generated
```

### Success Criteria
- ✓ No errors in the Console
- ✓ Duration value is reasonable (not 0 or negative)
- ✓ Sample rate is valid (typically 44100 or 48000 Hz)
- ✓ Sample count is greater than 0
- ✓ Waveform visualization appears (if UI is present)

## Troubleshooting

### Issue: "File not found" Error
**Cause:** The MP3 file path is incorrect or the file has been moved.

**Solution:**
1. Verify the file exists at `E:\Downloads\Heavy Is The Crown (Original Score).mp3`
2. Check for typos in the filename (note the spaces in the name)
3. Update the path in the Inspector if needed

### Issue: "NAudio.dll not found" Error
**Cause:** NAudio plugin is missing or not imported correctly.

**Solution:**
1. Check that `Assets/Plugins/NAudio.dll` exists
2. Reimport the NAudio.dll file if needed
3. Restart Unity Editor to refresh plugin references

### Issue: "Could not load MP3 file" Error
**Cause:** MP3 file may be corrupted or in an unsupported format.

**Solution:**
1. Verify the MP3 file plays in a media player (VLC, Windows Media Player)
2. Check file size (should be 1.6 MB)
3. Try converting the MP3 to a standard format (44.1kHz, 320kbps)
4. Test with a different MP3 file

### Issue: Script Compilation Errors
**Cause:** Missing dependencies or code issues.

**Solution:**
1. Check the Console for specific compilation errors
2. Verify all script files are present and not corrupted
3. Clear the Library folder and let Unity reimport (backup project first)
4. Restart Unity Editor

### Issue: Scene Doesn't Load or Is Empty
**Cause:** Scene file corruption or missing GameObjects.

**Solution:**
1. Check that `Assets/Scenes/MP3LoadTest.unity` is not corrupted
2. Look for the `MP3LoadTest` GameObject in the Hierarchy
3. If missing, create a new GameObject and attach the `MP3LoadTest` script
4. Configure the mp3FilePath in the Inspector

### Issue: No Console Output
**Cause:** Script may not be executing or Console is filtered.

**Solution:**
1. Check that the `MP3LoadTest` GameObject is active in the Hierarchy
2. Verify the script component is enabled in the Inspector
3. Clear Console filters (check "Collapse", "Clear on Play", etc.)
4. Add Debug.Log statements to verify code execution

## Performance Notes
- **File Size:** The test file is 1.6 MB, which should load quickly
- **Expected Load Time:** Less than 1 second on modern hardware
- **Memory Usage:** Approximately 5-10 MB for decoded audio data
- **Sample Count:** Expect several million samples for a typical song

## Next Steps After Successful Test
1. Verify waveform visualization displays correctly
2. Test with different MP3 files (various bitrates, sample rates)
3. Test error handling with invalid files
4. Profile memory usage during loading
5. Test on different platforms (Windows, Android if applicable)

## Additional Notes
- The test uses NAudio for MP3 decoding, which only works on Windows
- For other platforms, Unity's built-in AudioClip loading should be used
- The scene configuration has been pre-configured with the test file path
- Keep the Console window visible to monitor all debug output

## Support
If you encounter issues not covered in this guide:
1. Check the Unity Console for detailed error messages
2. Verify all file paths and dependencies
3. Review the MP3Loader.cs and MP3LoadTest.cs source code
4. Test with a simpler, known-good MP3 file

---

**Last Updated:** 2026-02-15  
**Project:** Desert Rider Unity Project  
**Test Scene:** MP3LoadTest.unity  
**Test File:** Heavy Is The Crown (Original Score).mp3
