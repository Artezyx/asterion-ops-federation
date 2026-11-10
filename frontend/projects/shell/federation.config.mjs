import { withNativeFederation, shareAll } from '@angular-architects/native-federation-v4/config';
export default withNativeFederation({
  name: 'shell',
  exposes: {},
  shared: { ...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto', build: 'package' }) },
  skip: ['rxjs/ajax','rxjs/fetch','rxjs/testing','rxjs/webSocket']
});
