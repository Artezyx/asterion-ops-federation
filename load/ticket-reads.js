import http from 'k6/http';
import { check, sleep } from 'k6';
export const options={scenarios:{tickets:{executor:'constant-vus',vus:10,duration:'30s'}},
  thresholds:{http_req_failed:['rate<0.01'],http_req_duration:['p(95)<800']}};
export default function(){
  if(!__ENV.JWT) throw new Error('Use a short-lived test JWT');
  const response=http.get((__ENV.BASE_URL||'http://localhost:5080')+'/api/v1/tickets',{
    headers:{Authorization:'Bearer '+__ENV.JWT}
  });
  check(response,{'authorized listing':r=>r.status===200});
  sleep(0.2);
}
