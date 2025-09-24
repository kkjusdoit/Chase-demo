const express = require('express');
const cors = require('cors');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(cors());
app.use(express.json());

// 数据文件路径
const dataFile = path.join(__dirname, 'leaderboard.json');

// 读取现有数据
let leaderboard = [];
try {
  if (fs.existsSync(dataFile)) {
    const data = fs.readFileSync(dataFile, 'utf8');
    leaderboard = JSON.parse(data);
  }
} catch (error) {
  console.log('创建新的排行榜文件');
  leaderboard = [];
}

// 健康检查
app.get('/', (req, res) => {
  res.json({ 
    message: 'Chase Game API Server Running!',
    leaderboard_count: leaderboard.length 
  });
});

// 提交分数
app.post('/submit-score', (req, res) => {
  try {
    const { device_id, player_name, score } = req.body;
    
    if (!device_id || score === undefined) {
      return res.status(400).json({
        success: false,
        error: 'device_id and score are required'
      });
    }

    // 检查是否已存在该设备的记录
    const existingIndex = leaderboard.findIndex(p => p.device_id === device_id);
    
    if (existingIndex !== -1) {
      // 如果新分数更高，则更新记录
      if (score > leaderboard[existingIndex].best_score) {
        leaderboard[existingIndex].best_score = score;
        leaderboard[existingIndex].player_name = player_name || 'Anonymous';
        leaderboard[existingIndex].updated_at = new Date().toISOString();
        
        // 保存到文件
        fs.writeFileSync(dataFile, JSON.stringify(leaderboard, null, 2));
        
        return res.json({
          success: true,
          message: 'New high score recorded!',
          new_record: true,
          score: score,
          previous_best: leaderboard[existingIndex].best_score
        });
      } else {
        return res.json({
          success: true,
          message: 'Score submitted, but not a new high score',
          new_record: false,
          current_best: leaderboard[existingIndex].best_score,
          submitted_score: score
        });
      }
    } else {
      // 新玩家，插入新记录
      leaderboard.push({
        device_id: device_id,
        player_name: player_name || 'Anonymous',
        best_score: score,
        created_at: new Date().toISOString(),
        updated_at: new Date().toISOString()
      });
      
      // 保存到文件
      fs.writeFileSync(dataFile, JSON.stringify(leaderboard, null, 2));
      
      return res.json({
        success: true,
        message: 'First score recorded!',
        new_record: true,
        score: score
      });
    }
  } catch (error) {
    console.error('Error in submit-score:', error);
    res.status(500).json({
      success: false,
      error: 'Internal server error'
    });
  }
});

// 获取排行榜
app.get('/leaderboard', (req, res) => {
  try {
    const limit = parseInt(req.query.limit) || 10;
    
    const topScores = leaderboard
      .sort((a, b) => b.best_score - a.best_score)
      .slice(0, limit)
      .map(player => ({
        player_name: player.player_name,
        best_score: player.best_score,
        created_at: player.created_at
      }));
    
    res.json({
      success: true,
      count: topScores.length,
      leaderboard: topScores
    });
  } catch (error) {
    console.error('Error in leaderboard:', error);
    res.status(500).json({
      success: false,
      error: 'Internal server error'
    });
  }
});

// 获取玩家个人分数
app.get('/player-score/:device_id', (req, res) => {
  try {
    const device_id = req.params.device_id;
    const player = leaderboard.find(p => p.device_id === device_id);
    
    if (player) {
      res.json({
        success: true,
        player: {
          player_name: player.player_name,
          best_score: player.best_score,
          created_at: player.created_at,
          updated_at: player.updated_at
        }
      });
    } else {
      res.json({
        success: true,
        player: null,
        message: 'No score found for this device'
      });
    }
  } catch (error) {
    console.error('Error in player-score:', error);
    res.status(500).json({
      success: false,
      error: 'Internal server error'
    });
  }
});

const PORT = 3000;
app.listen(PORT, () => {
  console.log(`🚀 Chase Game Server running on http://localhost:${PORT}`);
  console.log(`📊 Current leaderboard entries: ${leaderboard.length}`);
});
