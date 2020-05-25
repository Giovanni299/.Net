// Copyright (C) 2017 - PrimeStone S.A.S - David Valdés
//
// Only for research purpose
//
// Date of creation: 24-ene-2017
//
//This macro runs Newton-Rhapsons load flow algorithm, in order to generate simulated meter readings.

clear
clearglobal
clc
//**********************Variables globales*****************************//
global filePath filePath2 readingPath systemPath day day_f
global DL socket VARN VARL NODS LINS Y Ysh Z Vuns Vest GROUPING GBUSES GLINES
global SL PV PQ V Pgi Pdi Qgi Qdi  Scal lossesVector flowVector1 bdata readL
global flowVector2 C p NTL per readingPath  systemPath rsystemPath year month day
global n MVAbase systemFile systemName nhoras sc varac1 varac2 vareac1 vareac2 SYSTEM LoadParam
global MM dd fh noIns startDateDay outPutPath tipo expType C sysType lpVars ipVars nLp nIp lpLog ipLog multy intIntv
//********************************************************************//

format(10)//Se fija formato numérico en 10 digitos

filePath  = '..\..\Resourses';
filePath2  = 'D:\Repository_services\PrimeStone.ReadingsGenerator\PrimeStone.ReadingsGenerator\bin\Debug\PrimeSimulator';

diary('D:\Repository_services\PrimeStone.ReadingsGenerator\PrimeStone.ReadingsGenerator\bin\Debug\PrimeSimulator\Output\System_0\Trailfile.txt','new','prefix=YYYY-MM-DD hh:mm:ss');

function loadSettings()
    global filePath maxit tol MVAbase NTL LoadParam sysType systemPath systemName
    global year  month  day  nhoras noIns startDateDay day_f expType readingPath outPutPath
    global lpVars ipVars lpLog ipLog multy op intIntv

    if isfile(filePath+'\primeSimSettings.csv') then
        settings = csvRead(filePath+'\primeSimSettings.csv',';','.','string');

        maxit = evstr(settings(1,2));
        tol =  evstr(settings(2,2));
        MVAbase = evstr(settings(3,2));//Potencia base del sistema [MVA]
        NTL = evstr(settings(4,2));
        LoadParam = evstr(settings(5,2));
        sysType = settings(6,2);
        year = settings(7,2);
        month = evstr(settings(8,2));
        day = evstr(settings(9,2));
        startDateDay = day;
        day_f = evstr(settings(10,2));
        expType = evstr(settings(11,2));
        systemPath = settings(12,2);
        systemName = settings(13,2);
        readingPath = settings(14,2);
        outPutPath = settings(15,2);
        lpVars = settings(16,2);
        ipVars = settings(17,2);
        lpLog = evstr(settings(18,2));
        ipLog = evstr(settings(19,2));
        intIntv = evstr(settings(20,2));
        multy = evstr(settings(21,2));
        //        filepath
    else
        maxit = 100;
        tol =  1e-6;
        MVAbase = 100;
        NTL = 0;
        LoadParam = [];
        sysType = 'Pow';
        dt = datevec(datenum());
        year = string(dt(1));
        month = dt(2);
        day = dt(3) - 1;
        startDateDay = day;//param(3);
        day_f = day;
        expType = 2;
        systemPath = 'D:\Información David Valdés\PrimeSimLite\Systems\';
        systemName = 'PolishLP1.xls';
        outPutPath = 'D:\Información David Valdés\PrimeSimLite\Output\';
        lpVars = 'kWhD';
        ipVars = 'Vavg';
        lpLog = 1;
        ipLog = 1;
        intIntv = 60;
        multy = 1;
    end

    opjac=1;
    op=[tol maxit 1 opjac];
    ndays = day_f - day + 1;
    nhoras = ndays * 24;
    noIns = 60;

endfunction

//Calcula el ángulo de un número complejo
function a = angle(x)
    a=atan(imag(x),real(x));
endfunction

//Construye la matriz de admitancias nodales
function [Y,Ysh]=ybus(Z,C,D)

    Y = sparse(zeros(max(max(C(:,1)),max(C(:,2))),max(max(C(:,1)),max(C(:,2)))));
    Ysh = Y;

    for a=1:size(C,1)
        Zl(C(a,1),C(a,2)) = Z(a);
        Zl(C(a,2),C(a,1)) = Zl(C(a,1),C(a,2));
        if a > 1 then
            if C(a,1) == C(a-1,1) & C(a,2) == C(a-1,2) then
                Y(C(a,1),C(a,2)) = -1/Z(a) ;//+ Y(C(a-1,1),C(a-1,2));
            else
                Y(C(a,1),C(a,2)) = -1/Z(a);
            end
        else
            Y(C(a,1),C(a,2)) = -1/Z(a);
        end
        Ysh(C(a,1),C(a,2)) = D(a);
        Y(C(a,2),C(a,1)) = Y(C(a,1),C(a,2));
        Ysh(C(a,2),C(a,1))= Ysh(C(a,1),C(a,2));
    end

    for a=1:size(Y,1)
        Y(a,a) = -sum(Y(a,:));
    end
    Y = Y + Ysh;

endfunction

//Construye la matriz jacobiana de las ecuaciones de balance de potencia nodal
function jacs= jacobiano(V,Y,PV,PQ,jactype)
    funcprot(0)
    if jactype == 1 then
        //Construcción de la matriz jacobiana en coordenadas polares

        n = length(V);
        Ibus = Y* V;

        if issparse(Y)           // sparse version (if Y is sparse)
            diagV       = sparse([[1:n]', [1:n]'], V,[n, n]);
            diagIbus    = sparse([[1:n]', [1:n]'], Ibus,[n, n]);
            diagVnorm   = sparse([[1:n]', [1:n]'], V./abs(V), [n, n]);
        else                        // dense version
            diagV       = diag(V);
            diagIbus    = diag(Ibus);
            diagVnorm   = diag(V./abs(V));
        end

        DS_DV = diagV * conj(Y * diagVnorm) + conj(diagIbus) * diagVnorm;
        DS_DA = %i * diagV * conj(diagIbus - Y * diagV);
        DP_DA = real(DS_DA(gsort([PV;PQ],'r','i'), gsort([PV;PQ],'r','i')));
        DP_DV = real(DS_DV(gsort([PV;PQ],'r','i') , PQ));
        DQ_DA = imag(DS_DA(PQ , gsort([PV;PQ],'r','i')));
        DQ_DV = imag(DS_DV(PQ , PQ));
        Jp = sparse([DP_DA DP_DV;
        DQ_DA DQ_DV]);
        coord = "Polar coordinates";
        jacs = Jp;//list(coord,J,DS_DA,DS_DV);
    elseif jactype == 2
        //Construcción de la matriz jacobiana en coordenadas rectangulares
        e=real(V);
        f=imag(V);
        G=real(Y);
        B=imag(Y);
        G1=G; 
        B1=B;
        G1(PV,:)=0; 
        G1(:,PV)=0;
        G1(PV,PV)=1;
        B1(PV,:)=0; 
        B1(:,PV)=0;
        B1(PV,PV)=1;
        n=length(V);
        for i=1:n
            ai=0;
            ci=0;
            ai1=0;
            ci1=0;
            for j=1:n
                //                if i~=j then
                ai=ai+G(i,j)*e(j)-B(i,j)*f(j);
                ci=ci+G(i,j)*f(j)+B(i,j)*e(j);
                ai1=ai1+G1(i,j)*e(j)-B1(i,j)*f(j);
                ci1=ci1+G1(i,j)*f(j)+B1(i,j)*e(j);
                //                end
            end
            Ai(i)=ai;
            Ci(i)=ci;
            Ai1(i)=ai1;
            Ci1(i)=ci1;
        end

        for i=1:n
            for j=1:n
                if i==j then
                    dpe(i,j)=G(i,i)*e(i)+Ai(i)+B(i,i)*f(i);
                    dpf(i,j)=G(i,i)*f(i)+Ci(i)-B(i,i)*e(i);
                    dqe(i,j)=-B(i,i)*e(i)-Ci(i)+G(i,i)*f(i);
                    dqf(i,j)=-B(i,i)*f(i)+Ai(i)-G(i,i)*e(i);
                else
                    dpe(i,j)=G(i,j)*e(i)+B(i,j)*f(i);
                    dpf(i,j)=-B(i,j)*e(i)+G(i,j)*f(i);
                    dqe(i,j)=-B(i,j)*e(i)+G(i,j)*f(i);
                    dqf(i,j)=-G(i,j)*e(i)-B(i,j)*f(i);

                end
            end
        end

        dqe(PV,:)=0;
        dqf(PV,:)=0;
        dqe(PV,PV)=2*diag(e(PV));
        dqf(PV,PV)=2*diag(f(PV));
        Jr = sparse([dpe([PV;PQ],[PV;PQ]) dpf([PV;PQ],[PV;PQ]);dqe([PV;PQ],[PV;PQ]) dqf([PV;PQ],[PV;PQ])]);
        coord = "Rectangular coordinates";
        jacs = Jr;//list(coord,J,dpe,dpf,dqe,dqf);
    else
        //Valor de opción incorrecto
        disp('opjac solo permite valores iguales a 1 o 2')
        jacs = [];
        break
    end
endfunction

//Calcula multiplicador óptimo, Jacobiano y vector de correcciones 
function [u,da,dv,Jp] = mopol(V,Y,Scal,dS,SL,PQ,PV,sol)
    //	global eigen
    funcprot(0)
    u = ones(3,1);
    npv = length(PV);
    npq = length(PQ);
    P = real(Scal);
    Q = imag(Scal);
    dSP = dS(1:npv+npq);
    dSQ = dS(npv+npq+1:$);
    //**************************************************************************
    //Jacobiano

    Jp= jacobiano(V,Y,PV,PQ,1); 
    //Vector de correcciones
    Jfact = lufact(Jp);
    dxm = lusolve(Jfact,dS);
    ludel(Jfact);
    dam = dxm(1:npv+npq);
    dvm = dxm(npv+npq+1:$);
    //Cálculo del multiplicador óptimo
    if sol == 1 then
        a = dS;
    else
        a = zeros(length(dS),1);
    end
    b = -dS;
    c = -[P(gsort([PV;PQ],'r','i'));Q(PQ)];
    g0 = a'*b;
    g1 = b'*b+2*a'*c;
    g2 = 3*b'*c;
    g3 = 2*c'*c;
    Fu = [g3 g2 g1 g0];//Polinomio de segundo orden, cuyos mínimos son las soluciones del flujo de carga.
    if isnan(Fu) | isinf(Fu) then
        u = ones(3,1);
    elseif ~isnan(Fu) | ~isinf(Fu) 
        u = roots(Fu);
    end
    //    if or(imag(u) ~= 0) then
    //        u = u(find(imag(u) == 0));
    //    end
    da = [dam];
    dv = [dvm];
endfunction

//Cálculo de flujo de potencia
function varargout = powerFlow(V,Pesp,Qesp,Y,SL,PV,PQ,op)
    funcprot(0)
    global Jb eigen Vest;
    tol = op(1);
    maxit = op(2);
    met = 1;
    opjac = op(4);
    npv = length(PV);
    npq = length(PQ);
    iter = 0;
    convergio = 0;
    Sesp = [Pesp(gsort([PV;PQ],'r','i'));Qesp(PQ)];
    dS = ones(Sesp);
    opj = [2 1 2];
    //INICIO DEL FLUJO DE CARGA POR EL MÉTODO DE NEWTON RHAPSON EN COORDENADAS POLARES
    while max(abs(dS)) > tol

        iter = iter+1;
        if iter >= maxit then
            disp('Load flow doesn´t converge')
            break
        end
        //CALCULO DE LAS INYECCIONES DE POTENCIA
        I = Y*V;
        Scal = V.*conj(I);
        P = real(Scal);
        Q = imag(Scal);
        Scalc = [P(gsort([PV;PQ],'r','i'));Q(PQ)];
        //Vector de errores
        dS = Sesp-Scalc;
        //        [u,da,dv,Jb] = mopol(V,Y,Scal,dS,SL,PQ,PV,1);
        Jp= jacobiano(V,Y,PV,PQ,1); 
        //Vector de correcciones
        Jfact = lufact(Jp);
        dxm = lusolve(Jfact,dS);
        ludel(Jfact);
        da = dxm(1:npv+npq);
        dv = dxm(npv+npq+1:$);
        mpol =  1;//u(find(imag(u) == 0));
        Vm = abs(V);
        Va = angle(V);
        Vm(PQ) = Vm(PQ) + mpol*dv;
        //        Vm(PQ) = Vm(PQ).*(1 + mpol*dv);
        Va((gsort([PV;PQ],'r','i')))=(Va((gsort([PV;PQ],'r','i')))+mpol*da);//*180/%pi;
        V((gsort([PV;PQ],'r','i')))=Vm((gsort([PV;PQ],'r','i'))).*%e^(%i*Va((gsort([PV;PQ],'r','i'))));//(cos(Va*%pi/180)+%i *sin(Va*%pi/180));  Vm([PV;PQ]).*%e^(%i*Va([PV;PQ])*%pi/180)

        [de, dei] = max(abs(dS));
        indz = find(abs(V) == 0);
        V(indz) = 1;
    end //while 
    de = max(abs(dS));
    Vest = V;
    if de <= tol
        convergio=1;
        disp('Load flow converge in '+string(iter)+' iterations')
    end
    varargout=list(Vest,de,convergio,Scal,I,Jb,iter,da,dv);
    //Fin Newton-Raphson
endfunction

//Crea sockets
function createMeasurementSystem(SYSTEM)
    global socket bdata systemName sc varac1 varac2 vareac1 vareac2 socket Vbase lpVars ipVars intIntv
    global expType volt1 volt2 volt3 curr1 curr2 curr3 ph1 ph2 ph3 cph1 cph2 cph3 inst C sysType nLp nIp

    select expType
    case 1
        sep = ';';
        nvar = 4;
        select lpVars
        case 'kWhD'
            varLp = 'kWhD';
        case 'all'
            varLp = ['kWhD';'kWhR';'kVarhD';'kVarhR'];
        end
        varax = varLp;
    case 2
        sep = ',';
        select lpVars
        case 'kWhD'
            varLp = string([1]);
        case 'all'
            varLp = string([1;2;3;4]);
        end
        nLp = size(varLp,1);

        select ipVars
        case 'Vavg'
            varIp = string(5);
        case 'all'
            varIp = string([5;6;7;8;9;10;11;12;13;14;15;16]);
        end
        nIp = size(varIp,1);
        varax = [varLp;varIp];
        nvar = nLp + nIp;
    end

    onlyBusSockets = mlist([' ','bus','socket','variables','vBase'],bdata(:,1),string(zeros(length(bdata(:,1)),1)),string(zeros(length(bdata(:,1)),size(varax,1))),Vbase);

    select sysType
    case 'Pow'
        lineBusSockets = mlist([' ','bus','socket','variables','vBase'],[C(:,1),C(:,2)] ,string([C(:,1),C(:,2)]),string(zeros(size(length(bdata(:,1)) + 1:2*length(C(:,1))+ length(bdata(:,1)),2),size(varax,1))),ones(size(C(:,1))));

    case 'Dist'
        lineBusSockets = [];
    end

    if isempty(sc) then
        SC = SYSTEM(4);
        sckField = getfield(3,SC);
    end

    for  i=1:length(bdata(:,1))
        if isempty(sc) then
            onlyBusSockets.socket(i) = sckField(i);
            onlyBusSockets.variables(i,:) = varax';
        else
            onlyBusSockets.socket(i) = sc+string(i);
            onlyBusSockets.variables(i,:) = varax';
        end
    end
    save(filePath2+'\'+'onlyBusSockets_'+strsubst(systemName,'.xls','')+'.sod','onlyBusSockets');
    busSockets = mlist([' ','bus','socket','variable'],bdata(:,1),NODS,string(zeros(size(NODS,1),size(varax,1))));
    save(filePath2+'\'+'Bus_Sockets.sod','busSockets');
    if  ~isempty(lineBusSockets) then
        i1 = 0;
        for i= length(bdata(:,1)) + 1:2*length(C(:,1))+ length(bdata(:,1))

            i1 = i1+1;
            if isempty(sc) then
                lineBusSockets.socket(i1) = sckField(i);
                lineBusSockets.variables(i1,:) = varax';
            else
                lineBusSockets.socket(i1) = sc+string(i);
                lineBusSockets.variables(i1,:) = varax';
            end

        end

        for i = 1:length(bdata(:,1))
            bi = find(lineBusSockets.bus == onlyBusSockets.bus(i));
            lineBusSockets.vBase(bi) = onlyBusSockets.vBase(i);
        end


        save(filePath2+'\'+'lineBusSockets_'+strsubst(systemName,'.xls','')+'.sod','lineBusSockets');

        lineSockets = mlist([' ','bus','socket'],[C(:,1),C(:,2)] ,[LINS(1:size(LINS,1)/2), LINS(size(LINS,1)/2 + 1 :$)]);
        save(filePath2+'\'+'Line_Sockets.sod','lineSockets');

        socket = mlist([' ','bus','socket','variables'],[onlyBusSockets.bus;lineBusSockets.bus(:,1);lineBusSockets.bus(:,2)],...
        [onlyBusSockets.socket;lineBusSockets.socket(:,1);lineBusSockets.socket(:,2)],...
        [onlyBusSockets.variables;lineBusSockets.variables]);

        clear onlyBusSockets lineBusSockets
    else
        socket = mlist([' ','bus','socket','variables'],[onlyBusSockets.bus],...
        [onlyBusSockets.socket],...
        [onlyBusSockets.variables]);
    end
    save(filePath2+'\'+'Sockets_'+strsubst(systemName,'.xls','')+'.sod','socket');
endfunction

//Crea agrupaciones energéticas

//Mapea la información de las barras a variables del sistema
function buildBusData(SYSTEM)
    global bdata SL PV PQ V Pgi Pdi Qgi Qdi busStr busNum tipo Vbase
    PQ = [];
    PV = [];
    SL = [];
    Nodos=SYSTEM(1);
    bdata=getfield(4,Nodos);
    bdata(1,:)=[];
    if and(isnan(bdata(2,:))) then
        bdata=getfield(3,Nodos);
        bdata(1,:)=[];
        V=strtod(bdata(:,2));
        n=size(V,1);
        busStr = bdata(:,1);
        busNum = [1:n]';
        bdata(:,1) = string(busNum);
        tipo=strtod(bdata(:,7));

        Vbase = strtod(bdata(:,8));
        pv=0; 
        pq=0;
        Pgi=strtod(bdata(:,3));
        Qgi=strtod(bdata(:,4));
        Pdi=strtod(bdata(:,5));
        Qdi=strtod(bdata(:,6));
        bdata = strtod(bdata);
    else
        V=bdata(:,2);
        Vbase = bdata(:,8);
        n=size(V,1);
        if and(isnan(bdata(:,1))) then
            tmp = getfield(3,Nodos);
            tmp(1,:) = [];
            busStr = tmp(:,1);
            busNum = [1:n]';
            bdata(:,1) = busNum;
            clear tmp
        end
        tipo=bdata(:,7);
        pv=0; 
        pq=0;
        Pgi=bdata(:,3);
        Qgi=bdata(:,4);
        Pdi=bdata(:,5);
        Qdi=bdata(:,6);
    end
    //Clasificación de nodos
    for l=1:n
        if tipo(l)==1 then
            SL=l;         //Nodo Slack
        elseif tipo(l)==2
            pv=pv+1;
            PV(pv,1)=l;  //Nodos  PV
        elseif tipo(l)==3
            pq=pq+1;
            PQ(pq,1)=l; //Nodos PQ
        end
    end
    if pv==0 then
        PV=[];
    end
    Qc=0;
    //    V(SL) = V(SL)*%e^(%i*(-6.8*%pi/180));
endfunction

//Mapea la información de las líneas a variables del sistema
function buildLineData(SYSTEM)
    global Y bdata busStr busNum
    global Ysh Z
    global C
    nodos = busStr;
    n = size(nodos,1);
    Lineas=SYSTEM(2);
    L=getfield(4,Lineas);
    L(1,:)=[];
    if and(isnan(L(2,:))) then
        L=getfield(3,Lineas);
        L(1,:)=[];
        nini = L(:,1);
        nfin = L(:,2);
        nnini = zeros(size(nini,1),1);
        nnfin = zeros(size(nini,1),1);
        li = 0;
        lf = 0;
        for ll = 1:size(nini,1)
            for ni = 1:n
                if stripblanks(nini(ll)) == stripblanks(nodos(ni)) then
                    li = li + 1;
                    nnini(li)= busNum(ni);
                end
                if stripblanks(nfin(ll)) == stripblanks(nodos(ni)) then
                    lf = lf + 1;
                    nnfin(lf)= busNum(ni);
                end
            end
        end
        Z = ([strtod(L(:,3))+strtod(L(:,4))*%i]);
        C = [nnini nnfin];
        D = ([strtod(L(:,5))*%i]);
    else
        if and(isnan(L(:,1))) then
            tmp = getfield(3,Lineas);
            tmp(1,:) = [];
            nini = tmp(:,1);
            nfin = tmp(:,2);
            nnini = zeros(size(nini,1),1);
            nnfin = zeros(size(nini,1),1);
            li = 0;
            lf = 0;
            for ll = 1:size(nini,1)
                for ni =1:n
                    if stripblanks(nini(ll)) == stripblanks(nodos(ni)) then
                        li = li + 1;
                        nnini(li)= busNum(ni);
                    end
                    if stripblanks(nfin(ll)) == stripblanks(nodos(ni)) then
                        lf = lf + 1;
                        nnfin(lf)= busNum(ni);
                    end
                end
            end
            C = [nnini nnfin];
            clear tmp
        else
            C=[L(:,1) L(:,2)];
        end
        Z=[L(:,3)+L(:,4)*%i];
        D=[L(:,5)*%i];
    end
    [Y Ysh]=ybus(Z,C,D);
    clear L
endfunction

//Callback menú cálculo de flujo de potencia
function calcPowFlowCallback()
    //    clear Perdidas_Totales flowVector1 flowVector2 lossesVector ptosMed lp fh inst

    global systemFile systemName DL SL PV PQ V Pgi Pdi Qgi Pdi Qdi Y Ysh Scal
    global lossesVector flowVector1 flowVector2 C op NTL
    global fh per filePath2 nhoras bdata MVAbase socket 
    global Z Vuns Vest readL day day_f expType lpVars ipVars lpLog ipLog intIntv
    
    if isfile(filePath2+'\'+'onlyBusSockets_'+strsubst(systemName,'.xls','')+'.sod') then
        load(filePath2+'\'+'onlyBusSockets_'+strsubst(systemName,'.xls','')+'.sod','onlyBusSockets');
        VbaseB = onlyBusSockets.vBase;
    end
    
    if isfile(filePath2+'\'+'lineBusSockets_'+strsubst(systemName,'.xls','')+'.sod') then
        load(filePath2+'\'+'lineBusSockets_'+strsubst(systemName,'.xls','')+'.sod','lineBusSockets');
        VbaseL = lineBusSockets.vBase';
    end
    
    Perdidas_Totales = zeros(readL,1);
    flujoij = zeros(C(:,1));
    flujoji = zeros(C(:,1));


    lossesVector = zeros(C(:,1));
    vector_perd_bus = zeros(size(bdata,1),1);
    Scal = zeros(length(V),1); 

    if isempty(systemFile) then
        monthsagebox(gettext("Any system has been loaded."), gettext("Error"), "error", "modal");
        return
    else

        indperd = 0;
        lp = cell(readL,1);
        ptosMed = cell(readL,1);
        ptosMedIns = cell(readL,1);
        inst = cell(readL,1);
        fh = cell(readL,1);
        hour = 0;
        day = day;

//        winH = waitbar('Solving power flow, Please wait...');
        realtimeinit(0.1);

        if isempty(NTL) then
            NTL = 0;
        end

        V0 = V;

        for i = 1:readL

            Pg=DL(i)*Pgi;
            Qg=DL(i)*Qgi;
            Pd=(DL(i)+NTL).*Pdi;
            Qd=(DL(i)+NTL).*Qdi;
            Pesp=(Pg-Pd);
            Qesp=(Qg-Qd);
            Sesp=[Pesp(gsort([PV;PQ],'r','i'));Qesp(PQ)];
            cont = zeros(size(C,1),1);
            op = [op 1];
            //            V(PQ) = 1;
            [Vest,error_est,exito,Scal,I,Jb,iteraciones] = powerFlow(V,Pesp,Qesp,Y,SL,PV,PQ,op);
            V = Vest;

            if exito == 1 then
                indperd = indperd + 1;
                disp('Calculando flujos de potencia...')

                SFij = -diag(Vest(C(:,1))).*conj((Y(C(:,1),C(:,2))).*diag(Vest(C(:,1))- Vest(C(:,2))));
                SFji = -diag(Vest(C(:,2))).*conj((Y(C(:,2),C(:,1))).*diag(Vest(C(:,2))- Vest(C(:,1))));

                Vl = [Vest(C(:,1));Vest(C(:,2))];
                Il = full([diag((Y(C(:,1),C(:,2))).*diag(Vest(C(:,1))- Vest(C(:,2))));diag((Y(C(:,2),C(:,1))).*diag(Vest(C(:,2))- Vest(C(:,1))))]);

                cont = 1:size(C,1);
                flujoij1 = full(SFij);
                flujoij = diag(flujoij1);
                clear flujoij1

                flujoji1 = full(SFji);
                flujoji = diag(flujoji1);
                clear flujoji1

                lossesVector = flujoij + flujoji;
                Perdidas_Totales(i)=sum(lossesVector);

            else
                CALC_TIME(i)=timer();
                if CALC_TIME(i)>60 then
                    disp('El tiempo parcial de cálculo fue de '+string(CALC_TIME(i)/60)+' minutos')
                else
                    disp('El tiempo parcial de cálculo fue de '+string(CALC_TIME(i))+' segundos')
                end
            end

            CALC_TIME(i)=timer();
            //************************PRESENTACION DE LA INFORMACION**************************
            disp('*************************************************RESULTADOS********************************************************')
            disp('El flujo de carga convergió en '+string(iteraciones)+' iteraciones, con un error '+string(error_est)+', para una tolerancia de '+string(op(1)))

            if CALC_TIME(i)>60 then
                disp('El tiempo total de cálculo fue de '+string(CALC_TIME(i)/60)+' minutos')
            else
                disp('El tiempo total de cálculo fue de '+string(CALC_TIME(i))+' segundos')
            end

            realtime(i/readL);
//            waitbar(i/readL,winH);

            format(8)

            if sum(CALC_TIME)>60 then
                disp('El tiempo total de cálculo fue de '+string(sum(CALC_TIME)/60)+' minutos')
            else
                disp('El tiempo total de cálculo fue de '+string(sum(CALC_TIME))+' segundos')
            end

            Pc = real(Scal);
            Qc = imag(Scal);
            Pc(find(abs(real(Scal))*MVAbase <= 1e-4)) = 0;
            Qc(find(abs(imag(Scal))*MVAbase <= 1e-4)) = 0;
            Scali = Pc + %i*Qc;

            select sysType
            case 'Pow'
                
                select lpVars
                case 'kWhD'
                 
                    sLp = zeros(size(socket.socket,1),1);
                    sk = 1;
                    for si=1:size(bdata,1)
                        sLp(sk) = [(abs(real(Scali(si)))*MVAbase)];
                        sk = sk+1;
                    end

                    nLins = round((size(socket.socket,1) - size(bdata,1))/2);
                    for si = 1:nLins
                        //                    if sk < round((size(socket.socket,1) - size(bdata,1)))*4 then
                        sLp(sk) = [(abs(real(flujoij(si)))*MVAbase)];

                        //                    else
                        sLp(sk+nLins) = [(abs(real(flujoji(si)))*MVAbase)];
                        sk = sk+1;
                        //                    end
                    end

                case 'all'
                    sLp = zeros(size(socket.socket,1)*4,1);
                    sk = 1;
                    for si=1:size(bdata,1)
                        sLp(sk:sk+3) = [(abs(real(Scali(si)))*MVAbase);(abs(real(Scali(si))).*(1-NTL)*MVAbase);...
                        (abs(imag(Scali(si)))*MVAbase);(abs(imag(Scali(si)).*(1-NTL))*MVAbase)];
                        sk = sk+4;
                    end

                    nLins = round((size(socket.socket,1) - size(bdata,1))/2);
                    for si = 1:nLins
                        //                    if sk < round((size(socket.socket,1) - size(bdata,1)))*4 then
                        sLp(sk:sk+3) = [(abs(real(flujoij(si)))*MVAbase);(abs(real(flujoji(si))).*(1-NTL)*MVAbase);...
                        (abs(imag(flujoij(si)))*MVAbase);(abs(imag(flujoji(si))).*(1-NTL)*MVAbase)];

                        //                    else
                        sLp(sk+nLins*4:sk+nLins*4+3) = [(abs(real(flujoji(si)))*MVAbase);(abs(real(flujoij(si))).*(1-NTL)*MVAbase);...
                        (abs(imag(flujoji(si)))*MVAbase);(abs(imag(flujoij(si))).*(1-NTL)*MVAbase)];
                        sk = sk + 4;
                        //                    end
                    end

                end
                // lp{i} = string(sLp);
                
                lIp = mlist([' ','sockets','variables','readings'],socket.socket,socket.variables,zeros(size(socket.socket,1),size(lpVars,1)));
                strVA = [string(abs(V).*VbaseB);string(abs(Vl).*VbaseL)];//.*VbaseB
                strVB = [string(abs(V+grand(size(V,1), 1, "nor", 0.1, 0.01)).*VbaseB);string(abs(Vl+grand(size(Vl,1), 1, "nor", 0.1, 0.01)).*VbaseL)];
                strVC = [string(abs(V+grand(size(V,1), 1, "nor", 0.1, 0.01)).*VbaseB);string(abs(Vl+grand(size(Vl,1), 1, "nor", 0.1, 0.01)).*VbaseL)];
                strVavg = [string(median([V.*VbaseB abs(V+grand(size(V,1), 1, "nor", 0.1, 0.01)).*VbaseB abs(V+grand(size(V,1), 1, "nor", 0.1, 0.01)).*VbaseB],'c'));string(median([Vl.*VbaseL abs(Vl+grand(size(Vl,1), 1, "nor", 0.1, 0.01)).*VbaseL abs(Vl+grand(size(Vl,1), 1, "nor", 0.1, 0.01)).*VbaseL],'c'))];
                strIA = [string(abs(I).*(MVAbase./VbaseB));string(abs(Il).*(1000*MVAbase./VbaseL))];//
                strIB = [string(abs(I).*(MVAbase./VbaseB));string(abs(Il).*(1000*MVAbase./VbaseL))];
                strIC = [string(abs(I).*(MVAbase./VbaseB));string(abs(Il).*(1000*MVAbase./VbaseL))];
                strA = [string(angle(V)*%pi/180);string(angle(Vl)*%pi/180)];
                strB = [string((angle(V) + 120)*%pi/180);string((angle(Vl) + 120)*%pi/180)];
                strC = [string((angle(V) - 120)*%pi/180);string((angle(Vl) - 120)*%pi/180)];
                istrA = [string(angle(I)*%pi/180);string(angle(Il)*%pi/180)];
                istrB = [string((angle(I) + 120)*%pi/180);string((angle(Il) + 120)*%pi/180)];
                istrC = [string((angle(I) - 120)*%pi/180);string((angle(Il) - 120)*%pi/180)];


            case 'Dist'
                select lpVars
                case 'kWhD'
                    sLp = zeros(size(socket.socket,1)*1,1);
                    sk = 1;
                    for si=1:size(socket.socket,1)
                        sLp(sk) = [(abs(real(Scali(si)))*MVAbase)];
                        sk = sk+1;
                    end
                case 'all'
                    sLp = zeros(size(socket.socket,1)*4,1);
                    sk = 1;
                    for si=1:size(socket.socket,1)
                        sLp(sk:sk+3) = [(abs(real(Scali(si)))*MVAbase);(abs(real(Scali(si))).*(1-NTL)*MVAbase);...
                        (abs(imag(Scali(si)))*MVAbase);(abs(imag(Scali(si)).*(1-NTL))*MVAbase)];
                        sk = sk+4;
                    end
                end

                strVA = [string(abs(V).*VbaseB)];
                strVB = [string(abs(V)+grand(size(V,1), 1, "nor", 0.1, 0.01).*VbaseB)];
                strVC = [string(abs(V)+grand(size(V,1), 1, "nor", 0.1, 0.01).*VbaseB)];
                strVavg = [string(mean(abs(V)).*VbaseB)];
                strIA = [string(abs(I).*(MVAbase./VbaseB))];//
                strIB = [string(abs(I).*(MVAbase./VbaseB))];
                strIC = [string(abs(I).*(MVAbase./VbaseB))];
                strA = [string(angle(V)*%pi/180)];
                strB = [string((angle(V) + 120)*%pi/180)];
                strC = [string((angle(V) - 120)*%pi/180)];
                istrA = [string(angle(I)*%pi/180)];
                istrB = [string((angle(I) + 120)*%pi/180)];
                istrC = [string((angle(I) - 120)*%pi/180)];

            end

            if expType == 1 then
                lp{i} = [string(sLp)];
            else
                if lpLog == ipLog then
                    select lpLog
                    case 1
                        select ipVars
                        case 'Vavg'
                            lp{i} = [string(sLp);strVavg];
                        case 'all'
                            lp{i} = [string(sLp);[strVA;strVB;strVC]; [strIA;strIB;strIC]; [strA;strB;strC]; [istrA;istrB;istrC]];
                        end
                    case 2
                        select ipVars
                        case 'Vavg'
                            inst{i} = [string(sLp);strVavg];
                        case 'all'
                            inst{i} = [string(sLp);[strVA;strVB;strVC]; [strIA;strIB;strIC]; [strA;strB;strC]; [istrA;istrB;istrC]];
                        end
                    end
                else
                    lp{i} = string(sLp);
                    select ipVars
                    case 'Vavg'
                        inst{i} = strVavg;
                    case 'all'
                        inst{i} = [[strVA;strVB;strVC]; [strIA;strIB;strIC]; [strA;strB;strC]; [istrA;istrB;istrC]];
                    end
                end
            end



        end
        disp('Guardando en '+filePath2);
        save(filePath2+'\'+'LP_Raw_'+strsubst(systemName,'.xls','')+'.sod','lp');
        disp('Guardando en '+ filePath2+'\'+'LP_Raw_'+strsubst(systemName,'.xls','')+'.sod');
        save(filePath2+'\'+'INST_Raw_'+strsubst(systemName,'.xls','')+'.sod','inst');
        disp('Guardando en '+filePath2+'\'+'INST_Raw_'+strsubst(systemName,'.xls','')+'.sod');
        //close(winH)
        clear lp inst
    end
    
endfunction


function socketSettings()
    global filePath2 sc  varac1 varac2 vareac1 vareac2 lpVars ipVars
    global volt1 volt2 volt3 curr1 curr2 curr3 ph1 ph2 ph3 cph1 cph2 cph3 expType


    select expType 
    case 1
        if isfile(filePath2+'\'+'Sockets_Parameters_PGrid.sod') then
            load(filePath2+'\'+'Sockets_Parameters_PGrid.sod','scparam');
        else
            scparam = ['SOCKET';'kWhD';'kWhR';'kVarhD';'kVarhR'];
            save(filePath2+'\'+'Sockets_Parameters_PGrid.sod','scparam');
        end

        txt = ['Base name: ';'Active variable 1: ';'Active variable 2: ';'Reactive variable 1: ';'Reactive variable 2: '];
        scparam = x_mdialog('Enter socket parameters',txt,scparam);

        sc = scparam(1);
        varac1 = scparam(2);
        varac2 = scparam(3);
        vareac1 = scparam(4);
        vareac2 = scparam(5);

        save(filePath2+'\'+'Sockets_Parameters_PGrid.sod','scparam');

    case 2

        if isfile(filePath2+'\'+'Sockets_Parameters_PR.sod') then
            load(filePath2+'\'+'Sockets_Parameters_PR.sod','scparam');
        else
            scparam = ['SOCKET';'1';'2';'3';'4';'5';'6';'7';'8';'9';'10';'11';'12';'13';'14';'15';'16'];
            save(filePath2+'\'+'Sockets_Parameters_PR.sod','scparam');
        end

        sc = scparam(1);
        varac1 = scparam(2);
        varac2 = scparam(3);
        vareac1 = scparam(4);
        vareac2 = scparam(5);
        volt1 = scparam(6);
        volt2 = scparam(7);
        volt3 = scparam(8);
        curr1 = scparam(9);
        curr2 = scparam(10);
        curr3 = scparam(11);
        ph1 = scparam(12);
        ph2 = scparam(13);
        ph3= scparam(14);
        cph1 = scparam(15);
        cph2 = scparam(16);
        cph3= scparam(17);

        save(filePath2+'\'+'Sockets_Parameters_PR.sod','scparam');

    end

endfunction

//Función para cargar archivos
function openReadingFile()
    clear DL
    global readingPath  DL  LoadParam  MVAbase readL day day_f systemName multy intIntv
    filename = strsubst(strchr(systemName,'L'),'.xls','')+'.csv';
    LP=csvRead(readingPath+'\'+filename,';','.',"string");
    DL=strtod(LP(:,1))/median(strtod(LP(:,1)));
    if length([day*24:day_f*24*60/intIntv]) <= length(DL) then
        DL = strtod(LP(day*24:(day_f+1)*24*60/intIntv,1))/max(strtod(LP(:,1)));//;strtod(LP(day*24:(day_f+1)*24,1))/max(strtod(LP(:,1))*1.05);
    else
        disp("The selected time interval is beyond the time range of load profile data")
        messagebox("The selected time interval is beyond the time range of load profile data")
    end
    DL = (DL +  grand(size(DL,1), 1, "nor", 0.1, 0.01))*multy;
    //    else
    //DL = LoadParam;
    //    end

    if  length(DL) > 1 then
        readL = nhoras*60/intIntv;
    else
        readL = length(DL);
    end

endfunction

function dateTimeStr = createDateTimeStr(year,month,day,hour,minute,expType,hrlFileSz)
    global socket intIntv

    totSize = hrlFileSz;
    
    if month < 10 then
        MM=strcat(['0',string(month)]);
    else
        MM=string(month);
    end
    if day<10 then
        dd=strcat(['0',string(day)]);
    else
        dd=string(day);
    end

    if hour < 10 then
        h1 = hour;
        hh=strcat(['0',string(h1)]);
    else
        h1 = hour;
        hh=string(h1);
    end
    
    if minute < 10 then
        minuteStr='0'+string(minute);
    else
        minuteStr=string(minute);
    end
    

    hora=strcat([hh,minuteStr],':');        

    select expType
    case 1
        sou ='PrimeRead';
        rType = 'hour ';
        fecha=strcat([MM,dd,year],'/');
        estamp = strcat([fecha,hora],' ');
        dateTime = estamp(ones(totSize,1));
        source = sou(ones(totSize,1));
        readingType = rType(ones(totSize,1));
        dateTimeStr = [dateTime source readingType];
    case 2
        fecha=strcat([dd,MM,year],'/');
        estamp = strcat([fecha,hora],' ');
        dateTimeStr = ones(totSize,1);
        dateTimeStr = estamp(dateTimeStr)
    end
endfunction

function dateTimeStr = dtStrControl(year,month,day,hour,minute,expType,fileType)
    global socket nLp nIp lpLog ipLog intIntv 
    
    if expType == 1 then
        hrlFileSz = size(socket.variables,1)
        dateTimeStr = string(zeros(hrlFileSz,3));
    else
        if fileType == 1 then
            if lpLog == ipLog then
                hrlFileSz = size(socket.socket,1)*(nLp+nIp);
            else
                hrlFileSz = size(socket.socket,1)*nLp;
            end
        else
            if lpLog == ipLog then
                hrlFileSz = size(socket.socket,1)*(nLp+nIp);
            else
                hrlFileSz = size(socket.socket,1)*nIp;
            end
        end
        dateTimeStr = string(zeros(hrlFileSz,1));
    end

    k = 1;
    for i = 1:size(hour,2)
        if (month == 1 | month == 3 | month == 5 | month == 7 | month == 8 | month == 10) & day > 31 then
            day = 1;
            month = month+1;
        elseif month == 12 & day > 31
            day = 1;
            month = 1;
        elseif month == 2 & day > 28 
            day = 1;
            month = 3;
        elseif (month == 4 | month == 6 | month == 9 | month == 11) & day > 30
            day = 1;
            month = month + 1;
        end

        if hour == 24 then
            day = day+1;     
            hour=0;     
        end
        dateTimeStr(k:(hrlFileSz+k-1),:) = createDateTimeStr(year,month,day,hour,minute,expType,hrlFileSz);
        k = k + hrlFileSz;
    end


endfunction

function openSystemFile()
    global SYSTEM systemFile systemName systemPath bdata ldata Y Ysh
    systemFile = systemPath+'\'+systemName;
    SYSTEM = readxls(systemFile);
    buildBusData(SYSTEM);
    buildLineData(SYSTEM);
    createMeasurementSystem(SYSTEM);
endfunction


function exportFile()
    global  outPutPath expType bdata nLp nIp lpLog ipLog intIntv

    disp('Construyendo archivo de lecturas. Por favor espere...')

    BUILD_TIME=timer();

    //winH = waitbar('Bulding output files, Please wait...');
    realtimeinit(0.1);

    dayN = evstr(startDateDay) - 1;
    hourN = 24;

    select expType
    case 1
        load(filePath2+'\'+'LP_Raw_'+strsubst(systemName,'.xls','')+'.sod','lp');

        ptosMed = string(zeros(size(socket.socket,1)*size(socket.variables,2),2));
        tj = 1;
        for ti = 1:size(socket.socket,1)
            sci = socket.socket(ti);
            ptosMed(tj:tj+size(socket.variables,2)-1,:) = [sci(ones(size(socket.variables,2),1)),socket.variables(1,1:size(socket.variables,2))'];
            tj = tj + size(socket.variables,2);
        end

        head = ["Reading";
        "Socket Id;Variable Name;Value;Initial Date(yyyy/MM/dd HH:mm);SOURCE(AENC-SCADA-OPEN-CALCULADOS-DWH);READING_TYPE(Hour/Day/Month)"];
        hour = [0:23];
        minute = 0;
        for i = 1: day_f - evstr(startDateDay) +1


            kv = size(ptosMed,1);
            READING=string(zeros(kv*24,6));
            l = 1;
            lpc = 1;
            for j = 1:24
                dateTimeStr = dtStrControl(year,month,day,j-1,minute,expType,1);
                READING(l:kv+l-1,:)=[ ptosMed, cell2mat(lp(lpc)), dateTimeStr];
                l = kv+l ;
                lpc = lpc + 1;
            end

            ReadingFileName = outPutPath+'\Readings_'+strsubst(systemName,'.xls','')+ '_'+year+'-'+string(month)+'-'+string(dayN+1)+'.csv';
            csvWrite(READING,ReadingFileName, ';', '.',"%.30lg", head);
            realtime(i/(day_f - evstr(startDateDay) +1));
            //waitbar(i/(day_f - evstr(startDateDay) +1),winH);
            hourN = hourN + 24;
            dayN = dayN + 1;

        end



    case 2
        load(filePath2+'\'+'LP_Raw_'+strsubst(systemName,'.xls','')+'.sod','lp');
        load(filePath2+'\'+'INST_Raw_'+strsubst(systemName,'.xls','')+'.sod','inst');
        if sysType == 'Pow' then
            load(filePath2+'\'+'Line_Sockets.sod','lineSockets');
        end
        head = ['Id_medidor;Dst_activo;Fecha_lectura;Log;Canal;Valor'];


        szv = size(socket.variables(1,:),2);
        ptosMedLp = string(zeros(size(socket.socket,1)*nLp,4));
        ptosMedIns = string(zeros(size(socket.socket,1)*nIp,4));
        tj = 1;
       
        for ti = 1:size(socket.socket,1)
            sci = socket.socket(ti);
            vi = strtod(socket.variables(ti,:));
            ptosMedLp(tj:tj+nLp-1,:) = [sci(ones(nLp,1)),string(zeros(nLp,1)),string(lpLog*ones(nLp,1)),socket.variables(ti,find(vi<=4))'];
            tj = tj + nLp;
        end
        
        tj = 1;
        for ti = 1:size(socket.socket,1)
            sci = socket.socket(ti);
            vi = strtod(socket.variables(ti,:));
            ptosMedIns(tj:tj+nIp-1,:) = [sci(ones(nIp,1)),string(zeros(nIp,1)),string(ipLog*ones(nIp,1)),socket.variables(ti,find(vi> 4))'];
            tj = tj + nIp;
        end
        
        ptosMed = [ptosMedLp;ptosMedIns];
        
        select ipVars 
        case 'Vavg'
            insInd = find(ptosMed(:,3)=='2');
        case 'all'
            insInd = find(ptosMed(:,3)=='2');
        end

        lpInd = find(ptosMed(:,3)=='1');
        ipInd = find(ptosMed(:,3)=='2');

        lpc = 1;
        lpc1 = 1;
        dayN=day;
        for i = 1: day_f - evstr(startDateDay) +1
            
            kv = size(ptosMed(lpInd,1:2),1);
            READING1 = string(zeros(kv*24*60/intIntv,6));
            
            l = 1;
            hCount = 0;
            hour = 0;
            if intIntv == 60 then
                minute = 59;
            else
                minute = 0;
            end
            for j = 1:24*60/intIntv
                disp('Building LP readings for interval '+string(j)+' reading matrix');
                dateTimeStr = dtStrControl(year,month,dayN,hour,minute,expType,1);
                READING1(l:kv+l-1,:)=[ ptosMed(lpInd,1:2),dateTimeStr, ptosMed(lpInd,3:4), cell2mat(lp(lpc))];
                l = kv+l ;
                lpc = lpc + 1;
                hCount = hCount + 1;
                minute = minute + intIntv;
                if hCount == 60/intIntv then
                    hour = hour + 1;
                    hCount = 0;
                    if intIntv == 60 then
                        minute = 59;
                    else
                        minute = 0;
                    end
                end
            end
            
            if dayN < 10 then
                strDay = '0'+string(dayN);
            else
                strDay = string(dayN);
            end
            
//            READING = string(zeros(kv*24*60/intIntv,6)); 
//            READING = string(zeros(nLp*24*60/intIntv,6)); 
////            l = 1;
//            for ks = 1:size(socket.socket,1)
//                l = 1;
//                re = READING1(find(READING1(:,1)==socket.socket(ks)),:);
//                for ke = 1:size(socket.variables,2)
//                    rv = re(find(re(:,5)==socket.variables(ks,ke)),:);
//                    READING(l:l+size(rv,1)-1,:) = re(find(re(:,5)==socket.variables(ks,ke)),:);
//                    l = size(rv,1)+l;
//                end
//                ReadingFileName = outPutPath+'\LP_Readings_'+strsubst(socket.socket(ks),'.xls','')+ '_'+year+'-'+string(month)+'-'+strDay+'.csv';
//                csvWrite(READING,ReadingFileName, ';', ',','', head);
//            end
            
            
            
            ReadingFileName = outPutPath+'\LP_Readings_'+strsubst(systemName,'.xls','')+ '_'+year+'-'+string(month)+'-'+strDay+'.csv';
            
            csvWrite(READING1,ReadingFileName, ';', ',','', head);
           
            kv = size(ptosMed(insInd,1:2),1);
            READING2 = string(zeros(kv*24*60/intIntv,6));
            l = 1;
            
            hCount = 0;
            hour = 0;
            if intIntv == 60 then
                minute = 59;
            else
                minute = 0;
            end
            if nIp >= 1 & ~isempty(ipInd) then
                for j = 1:24*60/intIntv
                    disp('Building Instrumentation readings for hour '+string(j)+' reading matrix')
                    dateTimeStr = dtStrControl(year,month,dayN,hour,minute,expType,2);
                    READING2(l:kv+l-1,:)=[ptosMedIns(:,1:2),dateTimeStr, ptosMedIns(:,3:4), cell2mat(inst(lpc1))];
                    l = kv+l ;
                    lpc1 = lpc1 + 1;
                    hCount = hCount + 1;
                    minute = minute + intIntv;
                    if hCount == 60/intIntv then
                        hour = hour + 1;
                        hCount = 0;
                        if intIntv == 60 then
                            minute = 59;
                        else
                            minute = 0;
                        end
                    end

                end
            
            if dayN < 10 then
                strDay = '0'+string(dayN);
            else
                strDay = string(dayN);
            end
            
//            READING = string(zeros(kv*24*60/intIntv,6)); 
//            READING = string(zeros(nIp*24*60/intIntv,6));
////            l = 1;
//            for ks = 1:size(socket.socket,1)
//                l = 1;
//                re=READING2(find(READING2(:,1)==socket.socket(ks)),:);
//                for ke = 1:size(socket.variables,2)
//                    rv=re(find(re(:,5)==socket.variables(ks,ke)),:);
//                    READING(l:l+size(rv,1)-1,:) = re(find(re(:,5)==socket.variables(ks,ke)),:);
//                    l = size(rv,1)+l;
//                end
//                ReadingFileName = outPutPath+'\Inst_Readings_'+strsubst(socket.socket(ks),'.xls','')+ '_'+year+'-'+string(month)+'-'+strDay+'.csv';
//                csvWrite(READING,ReadingFileName, ';', ',',"%.30lg", head);
//            end
            
            
                ReadingFileName = outPutPath+'\Inst_Readings_'+strsubst(systemName,'.xls','')+ '_'+year+'-'+string(month)+'-'+strDay+'.csv';
                csvWrite(READING2,ReadingFileName, ';', ',',"%.30lg", head);
            end

            realtime(i/(day_f - evstr(startDateDay) +1));

//            waitbar(i/(day_f - evstr(startDateDay) +1),winH);

            hourN = hourN + 24;
            dayN = dayN + 1;
        end

    end

//    close(winH)
    BUILD_TIME=timer();
    if sum(BUILD_TIME) > 60 then
        disp('El tiempo total de construcción del archivo fue de '+string(sum(BUILD_TIME)/60)+' minutos ')
    else
        disp('El tiempo total de construcción del archivo fue de '+string(sum(BUILD_TIME))+' segundos ')
    end

endfunction

//Ejecución de la generación de lecturas
loadSettings();//Se cargan parámetros de la simulación
disp('Loading power system file. Please wait')
openSystemFile();//Se carga sistema de potencia
disp('Loading load pattern. Please wait')
openReadingFile();//Se carga curva de carga
disp('Simulating readings. Please wait')
calcPowFlowCallback();//Se calculan lecturas
disp('Readings simulation finished.')
disp('Exporting readings. Please wait')
exportFile();//Se exportan lecturas
disp('Readings exported.')
