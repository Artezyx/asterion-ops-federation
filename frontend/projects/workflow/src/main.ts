import { initFederation } from '@angular-architects/native-federation-v4';
initFederation({}, { hostRemoteEntry: { url: './remoteEntry.json' } })
  .then(() => import('./bootstrap'))
  .catch(err => console.error('Federation bootstrap failed', err));
