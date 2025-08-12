using System.Collections.Generic;
using RetroChess.AI;
using RetroChess.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class ChessGame {
    void Update() {
        HandleTimelineHotkeys();

        // 미리보기 중에는 라이브 입력 차단
        if (previewMode) { view.HideHoverIndicator(); lastHover = null; return; }

        // AI 턴에는 입력 차단
        if (isVsAI && gamePhase == GamePhase.Playing && board.SideToMove == aiSide) {
            view.HideHoverIndicator(); lastHover = null; return;
        }

        UpdateHover();
        if (waitingPromotion) return;

        var cam = Camera.main;
        if (cam == null || view == null) return;

        bool onUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // 우클릭 취소
        if (Input.GetMouseButtonDown(1)) {
            if (isDragging) {
                view.EndDrag(true);
                isDragging = false;
                dragCandidate = false;
                ClearSelection();
                return;
            }
            if (selected.HasValue || dragCandidate) {
                dragCandidate = false;
                ClearSelection();
                return;
            }
        }

        // MouseDown: 선택/이동/캡처(EP 허용) + 드래그 후보
        if (!onUI && Input.GetMouseButtonDown(0)) {
            var sq = view.ScreenToSquare(cam, Input.mousePosition);
            if (!(sq.x >= 0 && sq.x < 8 && sq.y >= 0 && sq.y < 8)) { dragCandidate = false; return; }

            if (selected == null) {
                var p = board.squares[sq.x, sq.y];
                if (!p.IsEmpty && (gamePhase == GamePhase.Free || p.Side == board.SideToMove)) {
                    // Free면 양쪽 다 선택 가능, Playing이면 내 차례만
                    TrySelect(sq);
                    dragCandidate = true; isDragging = false; dragFrom = sq; pressScreenPos = Input.mousePosition;
                } else {
                    ClearSelection();
                    dragCandidate = false;
                }
            } else {
                if (sq == selected.Value) {
                    ClearSelection();
                    dragCandidate = false;
                }
                else if (cachedMoves.Contains(sq) && board.squares[sq.x, sq.y].IsEmpty) {
                    bool endTurn = MovePiece(selected.Value, sq);
                    if (endTurn && gamePhase == GamePhase.Playing) EndTurn();
                    dragCandidate = false;
                }
                else {
                    bool isEpDest = board.EnPassantTarget.HasValue &&
                                    board.EnPassantTarget.Value.x == sq.x &&
                                    board.EnPassantTarget.Value.y == sq.y;

                    bool isNormalCapture = !board.squares[sq.x, sq.y].IsEmpty &&
                                           board.squares[sq.x, sq.y].Side != board.squares[selected.Value.x, selected.Value.y].Side;

                    if (cachedCaps.Contains(sq) && (isEpDest || isNormalCapture)) {
                        bool endTurn = MovePiece(selected.Value, sq);
                        if (endTurn && gamePhase == GamePhase.Playing) EndTurn();
                        dragCandidate = false;
                        return;
                    }

                    var p2 = board.squares[sq.x, sq.y];
                    if (!p2.IsEmpty && (gamePhase == GamePhase.Free || p2.Side == board.SideToMove)) {
                        TrySelect(sq);
                        dragCandidate = true; isDragging = false; dragFrom = sq; pressScreenPos = Input.mousePosition;
                    } else {
                        ClearSelection();
                        dragCandidate = false;
                    }
                }
            }
        }

        // MouseHold: 드래그 시작/업데이트
        if (Input.GetMouseButton(0) && dragCandidate) {
            float dist = Vector2.Distance((Vector2)pressScreenPos, (Vector2)Input.mousePosition);
            if (!isDragging && dist > dragStartPixels) {
                var moving = board.squares[dragFrom.x, dragFrom.y];
                var sp = view.GetPieceSprite(moving.Side, moving.Type);
                if (sp != null) {
                    view.BeginDrag(sp, dragFrom);
                    isDragging = true;
                }
            }
            if (isDragging) {
                var w = cam.ScreenToWorldPoint(Input.mousePosition);
                view.UpdateDrag(w);
            }
        }

        // MouseUp: 드래그 드롭(EP 포함)
        if (Input.GetMouseButtonUp(0)) {
            if (isDragging) {
                var dropSq = view.ScreenToSquare(cam, Input.mousePosition);

                bool isEpDest = board.EnPassantTarget.HasValue &&
                                board.EnPassantTarget.Value.x == dropSq.x &&
                                board.EnPassantTarget.Value.y == dropSq.y;

                bool canMove = cachedMoves.Contains(dropSq) && board.squares[dropSq.x, dropSq.y].IsEmpty;
                bool canCap  = cachedCaps.Contains(dropSq) && (
                                isEpDest ||
                                (!board.squares[dropSq.x, dropSq.y].IsEmpty &&
                                 board.squares[dropSq.x, dropSq.y].Side != board.squares[dragFrom.x, dragFrom.y].Side)
                              );

                if (canMove || canCap) {
                    bool endTurn = MovePiece(dragFrom, dropSq);
                    if (endTurn && gamePhase == GamePhase.Playing) EndTurn();
                }

                isDragging = false;
            }
            view.EndDrag(true);
            dragCandidate = false;
        }
    }

    // ←/→ 타임라인 단축키(요청 규칙 반영)
    void HandleTimelineHotkeys() {
        if (waitingPromotion) return;

        var es = EventSystem.current;
        if (es != null && es.currentSelectedGameObject != null) {
            if (es.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null) return;
            if (es.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null) return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            int n = TimelineLabels.Count;
            if (!previewMode) {
                if (n >= 2) EnterPreviewAt(n - 2);
                else if (n == 1) EnterPreviewStart();
            } else {
                if (previewIndex <= 0) EnterPreviewStart();
                else EnterPreviewAt(previewIndex - 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            int n = TimelineLabels.Count;
            if (previewMode) {
                int penultimate = n - 2;
                if (previewIndex <= penultimate - 1) EnterPreviewAt(previewIndex + 1);
                else ExitPreview();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (previewMode) ExitPreview();
        }
    }

    void UpdateHover() {
        var cam = Camera.main;
        if (cam == null || view == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            if (lastHover.HasValue) { view.HideHoverIndicator(); lastHover = null; }
            return;
        }

        var sq = view.ScreenToSquare(cam, Input.mousePosition);
        if (!(sq.x >= 0 && sq.x < 8 && sq.y >= 0 && sq.y < 8)) {
            if (lastHover.HasValue) { view.HideHoverIndicator(); lastHover = null; }
            return;
        }
        if (!lastHover.HasValue || lastHover.Value != sq) {
            view.ShowHoverIndicator(sq);
            lastHover = sq;
        }
    }

    void TrySelect(Vector2Int sq) {
        var p = board.squares[sq.x, sq.y];
        if (p.IsEmpty) return;
        if (gamePhase == GamePhase.Playing && p.Side != board.SideToMove) return;

        selected = sq;

        PseudoMoveGen.Generate(board, sq.x, sq.y, out var pseudoMoves, out var pseudoCaps);

        cachedMoves = new List<Vector2Int>();
        cachedCaps  = new List<Vector2Int>();
        foreach (var to in pseudoMoves) if (IsLegalAfter(board, sq, to)) cachedMoves.Add(to);
        foreach (var to in pseudoCaps)  if (IsLegalAfter(board, sq, to)) cachedCaps.Add(to);

        view.ShowSelection(sq);
        view.HighlightMoves(cachedMoves, cachedCaps);
    }
}