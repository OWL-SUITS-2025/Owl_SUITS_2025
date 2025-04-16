using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.UX;

public enum ProcedureStep
{
    StartVoiceRecording,
    SetupFieldNote,
    ScanSample,
    TakePhoto,
    FinishFieldNote,
    Complete,
    Completed
}