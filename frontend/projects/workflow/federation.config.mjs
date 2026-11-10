import { withNativeFederation, shareAll } from '@angular-architects/native-federation-v4/config';
export default withNativeFederation({
  name: 'workflow',
  exposes: {'./Component': './projects/workflow/src/app/workflow.component.ts'},
  shared: { ...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto', build: 'package' }) },
  skip: ['rxjs/ajax','rxjs/fetch','rxjs/testing','rxjs/webSocket']
});
