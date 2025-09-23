import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { VisitasComponent } from './components/visitas/visitas.component';
import { PersonasComponent } from './components/personas/personas.component';
import { LoginComponent } from './components/login/login.component'; 
import { ProductosComponent } from './components/productos/productos.component';
import { InformeTecnicoComponent } from './components/informe-tecnico/informe-tecnico.component';
import { ReunionesComponent } from './components/reuniones/reuniones.component';
import { authGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'visitas', component: VisitasComponent, canActivate: [authGuard] },
  { path: 'personas', component: PersonasComponent, canActivate: [authGuard] },
  { path: 'productos', component: ProductosComponent, canActivate: [authGuard] },
  { path: 'informe-tecnico', component: InformeTecnicoComponent, canActivate: [authGuard] },
  { path: 'reuniones', component: ReunionesComponent, canActivate: [authGuard] },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
