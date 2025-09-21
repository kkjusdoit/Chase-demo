export default {
  async fetch(request, env, ctx) {
    // 设置CORS头
    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type',
    };

    // 处理预检请求
    if (request.method === 'OPTIONS') {
      return new Response(null, { headers: corsHeaders });
    }

    const url = new URL(request.url);
    const path = url.pathname;

    // 调试信息
    console.log('Request path:', path);
    console.log('DB binding available:', !!env.DB);

    try {
      // 健康检查 - GET /
      if (path === '/' && request.method === 'GET') {
        return new Response(JSON.stringify({
          success: true,
          message: 'Chase Game API is running!',
          endpoints: [
            'POST /submit-score - 提交分数',
            'GET /leaderboard?limit=10 - 获取排行榜',
            'GET /player-score/{device_id} - 获取玩家分数'
          ]
        }), {
          headers: { ...corsHeaders, 'Content-Type': 'application/json' }
        });
      }

      // 检查数据库绑定
      if (!env.DB) {
        return new Response(JSON.stringify({
          success: false,
          error: 'D1 database not bound',
          message: '数据库未绑定，请检查Worker配置'
        }), {
          status: 500,
          headers: { ...corsHeaders, 'Content-Type': 'application/json' }
        });
      }

      // 获取排行榜 - GET /leaderboard
      if (path === '/leaderboard' && request.method === 'GET') {
        const limit = url.searchParams.get('limit') || '10';
        
        const stmt = env.DB.prepare(`
          SELECT player_name, best_score, created_at 
          FROM leaderboard 
          ORDER BY best_score DESC 
          LIMIT ?
        `).bind(parseInt(limit));
        
        const result = await stmt.all();
        
        return new Response(JSON.stringify({
          success: true,
          count: result.results.length,
          leaderboard: result.results
        }), {
          headers: { ...corsHeaders, 'Content-Type': 'application/json' }
        });
      }

      // 提交分数 - POST /submit-score
      if (path === '/submit-score' && request.method === 'POST') {
        const body = await request.json();
        const { device_id, player_name, score } = body;

        if (!device_id || score === undefined) {
          return new Response(JSON.stringify({
            success: false,
            error: 'device_id and score are required'
          }), {
            status: 400,
            headers: { ...corsHeaders, 'Content-Type': 'application/json' }
          });
        }

        // 检查是否已存在该设备的记录
        const existingRecord = await env.DB.prepare(`
          SELECT id, best_score FROM leaderboard WHERE device_id = ?
        `).bind(device_id).first();

        let result;
        
        if (existingRecord) {
          // 如果新分数更高，则更新记录
          if (score > existingRecord.best_score) {
            result = await env.DB.prepare(`
              UPDATE leaderboard 
              SET best_score = ?, player_name = ?, updated_at = CURRENT_TIMESTAMP 
              WHERE device_id = ?
            `).bind(score, player_name || 'Anonymous', device_id).run();
            
            return new Response(JSON.stringify({
              success: true,
              message: 'New high score recorded!',
              new_record: true,
              score: score,
              previous_best: existingRecord.best_score
            }), {
              headers: { ...corsHeaders, 'Content-Type': 'application/json' }
            });
          } else {
            return new Response(JSON.stringify({
              success: true,
              message: 'Score submitted, but not a new high score',
              new_record: false,
              current_best: existingRecord.best_score,
              submitted_score: score
            }), {
              headers: { ...corsHeaders, 'Content-Type': 'application/json' }
            });
          }
        } else {
          // 新玩家，插入新记录
          result = await env.DB.prepare(`
            INSERT INTO leaderboard (device_id, player_name, best_score) 
            VALUES (?, ?, ?)
          `).bind(device_id, player_name || 'Anonymous', score).run();
          
          return new Response(JSON.stringify({
            success: true,
            message: 'First score recorded!',
            new_record: true,
            score: score
          }), {
            headers: { ...corsHeaders, 'Content-Type': 'application/json' }
          });
        }
      }

      // 获取玩家个人最佳分数 - GET /player-score/{device_id}
      if (path.startsWith('/player-score/') && request.method === 'GET') {
        const device_id = path.split('/')[2];
        
        if (!device_id) {
          return new Response(JSON.stringify({
            success: false,
            error: 'Device ID is required'
          }), {
            status: 400,
            headers: { ...corsHeaders, 'Content-Type': 'application/json' }
          });
        }
        
        const result = await env.DB.prepare(`
          SELECT player_name, best_score, created_at, updated_at 
          FROM leaderboard 
          WHERE device_id = ?
        `).bind(device_id).first();
        
        if (result) {
          return new Response(JSON.stringify({
            success: true,
            player: result
          }), {
            headers: { ...corsHeaders, 'Content-Type': 'application/json' }
          });
        } else {
          return new Response(JSON.stringify({
            success: true,
            player: null,
            message: 'No score found for this device'
          }), {
            headers: { ...corsHeaders, 'Content-Type': 'application/json' }
          });
        }
      }

      // 404 - 路径不存在
      return new Response(JSON.stringify({
        success: false,
        error: 'Endpoint not found',
        path: path,
        method: request.method
      }), {
        status: 404,
        headers: { ...corsHeaders, 'Content-Type': 'application/json' }
      });

    } catch (error) {
      console.error('Error:', error);
      return new Response(JSON.stringify({
        success: false,
        error: 'Internal server error',
        message: error.message
      }), {
        status: 500,
        headers: { ...corsHeaders, 'Content-Type': 'application/json' }
      });
    }
  }
}; 