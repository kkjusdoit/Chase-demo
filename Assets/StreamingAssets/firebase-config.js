// Firebase WebGL Configuration
// 这个文件为WebGL平台提供Firebase配置
const firebaseConfig = {
  apiKey: "AIzaSyAAgtl1W9xncW8WgOuyEFdy9X096vsEBzk",
  authDomain: "fir-49cdb.firebaseapp.com",
  databaseURL: "https://fir-49cdb-default-rtdb.asia-southeast1.firebasedatabase.app",
  projectId: "fir-49cdb",
  storageBucket: "fir-49cdb.firebasestorage.app",
  messagingSenderId: "815612507894",
  appId: "1:815612507894:web:fe9bfa40631f7232d49ed7" // 使用相同的appId但修改平台标识
};

// 导出配置供Unity WebGL使用
window.firebaseConfig = firebaseConfig; 