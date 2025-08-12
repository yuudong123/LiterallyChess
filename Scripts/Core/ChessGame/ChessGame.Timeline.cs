using System.Collections.Generic;
using RetroChess.Core;
using UnityEngine;

public partial class ChessGame
{
    void AddTimelineEntry(string label)
    {
        timeline.Add(Snapshot.Capture(board));
        timelineLabels.Add(label);
        NotifyTimelineChanged();
    }

    void EnterPreviewAt(int index)
    {
        if (timeline.Count == 0) return;
        previewMode = true;
        previewIndex = Mathf.Clamp(index, 0, timeline.Count - 1);

        Snapshot.Restore(previewBoard, timeline[previewIndex]);
        view.RenderAll(previewBoard);

        view.SetTimelinePreview(true);
        view.HideCheck(); view.HideResult();
        NotifyTimelineChanged();
    }

    void EnterPreviewStart()
    {
        previewMode = true;
        previewIndex = -1;

        Snapshot.Restore(previewBoard, startSnapshot);
        view.RenderAll(previewBoard);

        view.SetTimelinePreview(true);
        view.HideCheck(); view.HideResult();
        NotifyTimelineChanged();
    }

    void ExitPreview()
    {
        previewMode = false;
        previewIndex = -1;

        view.RenderAll(board);
        view.SetTimelinePreview(false);
        UpdateCheckAndEnd();
        NotifyTimelineChanged();

        TryStartAITurn();
    }

    public void OnClickViewFirst() { EnterPreviewStart(); }

    public void OnClickViewPrev()
    {
        int n = timelineLabels.Count;
        if (n == 0) return;

        if (!previewMode)
        {
            if (n >= 2) EnterPreviewAt(n - 2);
            else EnterPreviewStart();
        }
        else
        {
            if (previewIndex <= 0) EnterPreviewStart();
            else EnterPreviewAt(previewIndex - 1);
        }
    }

    public void OnClickViewNext()
    {
        int n = timelineLabels.Count;
        if (n == 0) return;

        if (previewMode)
        {
            int penultimate = n - 2;
            if (previewIndex <= penultimate - 1) EnterPreviewAt(previewIndex + 1);
            else ExitPreview();
        }
    }

    public void OnClickViewLast() { ExitPreview(); }

    public void OnClickTimelineItem(int index) { EnterPreviewAt(index); }
}