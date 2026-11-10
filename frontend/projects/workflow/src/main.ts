import { initFederation } from '@angular-architects/native-federation-v4';
initFederation({})
  .then(() => import('./bootstrap'))
  .catch(err => console.error('Federation bootstrap failed', err));
