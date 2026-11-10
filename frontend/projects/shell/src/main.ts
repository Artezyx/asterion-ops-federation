import { initFederation } from '@angular-architects/native-federation-v4';
initFederation('federation.manifest.json')
  .then(() => import('./bootstrap'))
  .catch(err => console.error('Federation bootstrap failed', err));
