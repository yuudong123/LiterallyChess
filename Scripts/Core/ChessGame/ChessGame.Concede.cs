// Assets/Scripts/Game/Chess/ChessGame.Concede.cs
using RetroChess.Core;

public partial class ChessGame {
    // 사람이 'Give up(기권)'을 눌렀을 때: 즉시 상대 승리로 종료
    public void ConcedeByHuman() {
        if (gamePhase == GamePhase.Ended) return;

        // 승자: vs AI면 AI 색, PvP/Free면 내 선호색 반대
        Side winner = isVsAI ? aiSide
                             : (preferredHumanSide == Side.White ? Side.Black : Side.White);

        // 아래 헬퍼(ForceGameOverByConcede)는 ChessGame.State.cs에 이미 존재하는 전제입니다.
        // 만약 없다면 본문 하단의 대안(B)을 적용하세요.
        ForceGameOverByConcede(winner, isVsAI);
    }
}