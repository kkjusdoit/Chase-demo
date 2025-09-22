// Cloudflare Pages Functions - Unity WebGL MIME Type Handler
export async function onRequestGet({ request, env, params }) {
  const url = new URL(request.url);
  
  // Add CORS headers for all responses
  const corsHeaders = {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET, HEAD, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type, Range',
  };
  
  // Handle Unity WebGL files with correct MIME types and compression
  if (url.pathname.endsWith('.wasm.unityweb')) {
    const response = await env.ASSETS.fetch(request);
    return new Response(response.body, {
      status: response.status,
      statusText: response.statusText,
      headers: {
        ...Object.fromEntries(response.headers),
        ...corsHeaders,
        'Content-Type': 'application/wasm',
        'Content-Encoding': 'gzip',
        'Cache-Control': 'public, max-age=31536000, immutable',
        'Vary': 'Accept-Encoding',
      },
    });
  }
  
  if (url.pathname.endsWith('.js.unityweb')) {
    const response = await env.ASSETS.fetch(request);
    return new Response(response.body, {
      status: response.status,
      statusText: response.statusText,
      headers: {
        ...Object.fromEntries(response.headers),
        ...corsHeaders,
        'Content-Type': 'application/javascript',
        'Content-Encoding': 'gzip',
        'Cache-Control': 'public, max-age=31536000, immutable',
        'Vary': 'Accept-Encoding',
      },
    });
  }
  
  if (url.pathname.endsWith('.data.unityweb') || url.pathname.endsWith('.unityweb')) {
    const response = await env.ASSETS.fetch(request);
    return new Response(response.body, {
      status: response.status,
      statusText: response.statusText,
      headers: {
        ...Object.fromEntries(response.headers),
        ...corsHeaders,
        'Content-Type': 'application/octet-stream',
        'Content-Encoding': 'gzip',
        'Cache-Control': 'public, max-age=31536000, immutable',
        'Vary': 'Accept-Encoding',
      },
    });
  }
  
  // Handle regular JavaScript files
  if (url.pathname.endsWith('.js')) {
    const response = await env.ASSETS.fetch(request);
    return new Response(response.body, {
      status: response.status,
      statusText: response.statusText,
      headers: {
        ...Object.fromEntries(response.headers),
        ...corsHeaders,
        'Content-Type': 'application/javascript',
        'Cache-Control': 'public, max-age=31536000',
      },
    });
  }
  
  // For all other requests, just add CORS headers and pass through
  const response = await env.ASSETS.fetch(request);
  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers: {
      ...Object.fromEntries(response.headers),
      ...corsHeaders,
    },
  });
}

// Handle preflight requests
export async function onRequestOptions({ request }) {
  return new Response(null, {
    status: 200,
    headers: {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, HEAD, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Range',
      'Access-Control-Max-Age': '86400',
    },
  });
}
