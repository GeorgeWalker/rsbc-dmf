/* tslint:disable */
/* eslint-disable */
import { HttpClient, HttpContext, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { StrictHttpResponse } from '../../strict-http-response';
import { RequestBuilder } from '../../request-builder';

import { Document } from '../../models/document';

export interface ApiDocumentDriverIdAllDocumentsGet$Plain$Params {
  driverId: string;
}

export function apiDocumentDriverIdAllDocumentsGet$Plain(http: HttpClient, rootUrl: string, params: ApiDocumentDriverIdAllDocumentsGet$Plain$Params, context?: HttpContext): Observable<StrictHttpResponse<Array<Document>>> {
  const rb = new RequestBuilder(rootUrl, apiDocumentDriverIdAllDocumentsGet$Plain.PATH, 'get');
  if (params) {
    rb.path('driverId', params.driverId, {});
  }

  return http.request(
    rb.build({ responseType: 'text', accept: 'text/plain', context })
  ).pipe(
    filter((r: any): r is HttpResponse<any> => r instanceof HttpResponse),
    map((r: HttpResponse<any>) => {
      return r as StrictHttpResponse<Array<Document>>;
    })
  );
}

apiDocumentDriverIdAllDocumentsGet$Plain.PATH = '/api/Document/{driverId}/AllDocuments';
