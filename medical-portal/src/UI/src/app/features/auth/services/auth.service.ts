import { Injectable } from '@angular/core';
import { Observable, from, of } from 'rxjs';
import { KeycloakService } from 'keycloak-angular';
import { KeycloakLoginOptions } from 'keycloak-js';
import { Role } from '../enums/identity-provider.enum';
import { ProfileManagementService } from '@app/shared/services/profile.service';
import { SESSION_STORAGE_KEYS } from '@app/app.model';

export interface IAuthService {
  login(options?: KeycloakLoginOptions): Observable<void>;
  isLoggedIn(): Observable<boolean>;
  logout(redirectUri: string): Observable<void>;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService implements IAuthService {
  public constructor(
    private keycloakService: KeycloakService,
    private profileManagementService: ProfileManagementService
  ) {}

  public login(options?: KeycloakLoginOptions): Observable<void> {
    return from(this.keycloakService.login(options));
  }

  public getHpdid(): string | undefined {
    const idTokenParsed = this.keycloakService.getKeycloakInstance().idTokenParsed;
    if (idTokenParsed !== undefined) {
      return idTokenParsed['preferred_username'];
    }
    return undefined;
  }

  public isLoggedIn(): Observable<boolean> {
    return of(this.keycloakService.isLoggedIn());
  }

  // NOTE everyone that can login has access to all pages e.g. Practitioner, MOA/MOM
  // MOA/MOM will have restricted access on each page with feature level permissions
  public hasAccess(): boolean {
    //console.info('getUserRoles', this.keycloakService.getUserRoles());
    //return this.keycloakService.isLoggedIn() && this.keycloakService.isUserInRole(Role.Enrolled);
    //return !!this.profileManagementService.getCachedProfile().roles?.includes(Role.Enrolled);
    return true;
  }

  public getRoles(): Role[] {
    const roleNames = this.keycloakService
      .getUserRoles()
      .filter((role) => role !== Role.Enrolled);
    // TODO roles should come from Api and Api should add MOA role, for now just use MOA role if no other role exists
    if (roleNames.length == 0)
      roleNames.push(Role.Moa);
    return roleNames
      .map((role) => Object.values(Role).find((value) => value === role))
      .filter((role) => role !== undefined) as Role[];
  }

  public logout(redirectUri: string): Observable<void> {
    sessionStorage.removeItem(SESSION_STORAGE_KEYS.PROFILE);
    return from(this.keycloakService.logout(redirectUri));
  }
}
